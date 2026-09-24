using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class NotesService(NotesDbContext db, IEventPublisher eventPublisher) : INotesService
{
    public async Task<NoteResponse> CreateAsync(Guid userId, CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now,
            // Triggers AI Categorization on this same save when there's real content
            // already (BR-008/BR-010) - never again afterwards (User Ownership, ticket 025).
            AiCategorizationTriggeredAt = hasContent ? now : null,
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteCreated(note.Id, note.UserId, note.Content, note.CreatedAt), cancellationToken);
        if (hasContent)
            await eventPublisher.PublishAsync(new NoteReadyForCategorization(note.Id, note.UserId, note.Content, now), cancellationToken);

        return ToResponse(note, []);
    }

    public async Task<NoteResponse?> GetByIdAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.AsNoTracking()
            .SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId, cancellationToken);
        if (note is null)
            return null;

        var categoryIds = await CategoryIdsForAsync(noteId, cancellationToken);
        return ToResponse(note, categoryIds);
    }

    public async Task<IReadOnlyList<NoteResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notes = await db.Notes.AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt == null)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

        return await ToResponsesAsync(notes, cancellationToken);
    }

    public async Task<IReadOnlyList<NoteResponse>> ListTrashAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notes = await db.Notes.AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt != null)
            .OrderByDescending(n => n.DeletedAt)
            .ToListAsync(cancellationToken);

        return await ToResponsesAsync(notes, cancellationToken);
    }

    public async Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return null;

        // Overwrite, not append - Notes have no version history (GLOSSARY.md).
        note.Content = request.Content;
        note.UpdatedAt = DateTimeOffset.UtcNow;

        // The Note may have been created empty (capture-first) and only just received real
        // content - that's still "the first save with real content" (BR-008), so it still
        // triggers, exactly once, same as on Create.
        var triggersNow = note.AiCategorizationTriggeredAt is null && !string.IsNullOrWhiteSpace(note.Content);
        if (triggersNow)
            note.AiCategorizationTriggeredAt = note.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteUpdated(note.Id, note.UserId, note.Content, note.UpdatedAt), cancellationToken);
        if (triggersNow)
            await eventPublisher.PublishAsync(new NoteReadyForCategorization(note.Id, note.UserId, note.Content, note.UpdatedAt), cancellationToken);

        var categoryIds = await CategoryIdsForAsync(noteId, cancellationToken);
        return ToResponse(note, categoryIds);
    }

    public async Task<bool> TriggerCategorizationAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.AsNoTracking()
            .SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return false;

        // Deliberately does not touch AiCategorizationTriggeredAt - this on-demand action is
        // separate from, and doesn't affect, the one-time automatic trigger (ticket 025).
        await eventPublisher.PublishAsync(new NoteReadyForCategorization(note.Id, note.UserId, note.Content, DateTimeOffset.UtcNow), cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return false;

        note.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteDeleted(note.Id, note.UserId, note.DeletedAt.Value), cancellationToken);

        return true;
    }

    public async Task<bool> RestoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt != null, cancellationToken);
        if (note is null)
            return false;

        note.DeletedAt = null;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AssignNoteTopicResult> AssignTopicAsync(Guid userId, Guid noteId, Guid? topicId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return AssignNoteTopicResult.Failure(AssignNoteTopicFailureReason.NoteNotFound);

        if (topicId is { } newTopicId)
        {
            // The Topic must already exist (ticket 036) - validated against Notes' own
            // KnownTopic read model, not by querying the organization schema (ADR-0003).
            var topicExists = await db.KnownTopics.AnyAsync(t => t.TopicId == newTopicId && t.UserId == userId, cancellationToken);
            if (!topicExists)
                return AssignNoteTopicResult.Failure(AssignNoteTopicFailureReason.TopicNotFound);
        }

        var oldTopicId = note.TopicId;
        if (oldTopicId == topicId)
            return AssignNoteTopicResult.Success(await ToResponseWithCategoriesAsync(note, cancellationToken));

        note.TopicId = topicId;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteTopicAssigned(note.Id, note.UserId, oldTopicId, topicId, note.UpdatedAt), cancellationToken);

        return AssignNoteTopicResult.Success(await ToResponseWithCategoriesAsync(note, cancellationToken));
    }

    public async Task<NoteCategoryAttachResult> AttachCategoryAsync(Guid userId, Guid noteId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return NoteCategoryAttachResult.Failure(NoteCategoryAttachFailureReason.NoteNotFound);

        // The Category must already exist - validated against Notes' own KnownCategory
        // read model, not by querying the organization schema (ADR-0003).
        var categoryExists = await db.KnownCategories.AnyAsync(c => c.CategoryId == categoryId && c.UserId == userId, cancellationToken);
        if (!categoryExists)
            return NoteCategoryAttachResult.Failure(NoteCategoryAttachFailureReason.CategoryNotFound);

        var alreadyAttached = await db.NoteCategories.AnyAsync(nc => nc.NoteId == noteId && nc.CategoryId == categoryId, cancellationToken);
        if (!alreadyAttached)
        {
            db.NoteCategories.Add(new NoteCategory { NoteId = noteId, CategoryId = categoryId });
            await db.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(new NoteCategoryAttached(noteId, userId, categoryId, DateTimeOffset.UtcNow), cancellationToken);
        }

        return NoteCategoryAttachResult.Success(await ToResponseWithCategoriesAsync(note, cancellationToken));
    }

    public async Task<NoteResponse?> DetachCategoryAsync(Guid userId, Guid noteId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return null;

        var link = await db.NoteCategories.SingleOrDefaultAsync(nc => nc.NoteId == noteId && nc.CategoryId == categoryId, cancellationToken);
        if (link is not null)
        {
            db.NoteCategories.Remove(link);
            await db.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(new NoteCategoryDetached(noteId, userId, categoryId, DateTimeOffset.UtcNow), cancellationToken);
        }

        return await ToResponseWithCategoriesAsync(note, cancellationToken);
    }

    private Task<List<Guid>> CategoryIdsForAsync(Guid noteId, CancellationToken cancellationToken) =>
        db.NoteCategories.AsNoTracking()
            .Where(nc => nc.NoteId == noteId)
            .Select(nc => nc.CategoryId)
            .ToListAsync(cancellationToken);

    private async Task<NoteResponse> ToResponseWithCategoriesAsync(Note note, CancellationToken cancellationToken) =>
        ToResponse(note, await CategoryIdsForAsync(note.Id, cancellationToken));

    private async Task<IReadOnlyList<NoteResponse>> ToResponsesAsync(List<Note> notes, CancellationToken cancellationToken)
    {
        var noteIds = notes.Select(n => n.Id).ToList();
        var categoriesByNoteId = await db.NoteCategories.AsNoTracking()
            .Where(nc => noteIds.Contains(nc.NoteId))
            .GroupBy(nc => nc.NoteId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(nc => nc.CategoryId).ToList(), cancellationToken);

        return notes.Select(n => ToResponse(n, categoriesByNoteId.GetValueOrDefault(n.Id, []))).ToList();
    }

    private static NoteResponse ToResponse(Note note, IReadOnlyList<Guid> categoryIds) => new()
    {
        Id = note.Id,
        Content = note.Content,
        TopicId = note.TopicId,
        CategoryIds = categoryIds,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt,
        DeletedAt = note.DeletedAt,
    };
}
