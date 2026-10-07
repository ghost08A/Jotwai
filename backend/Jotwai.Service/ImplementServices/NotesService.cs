using System.Data;
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
    // Matches the Name column's max length in NoteConfiguration.
    private const int MaxNameLength = 200;

    public async Task<NoteWriteResult> CreateAsync(Guid userId, CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(request.Name);
        if (name is null)
            return NoteWriteResult.Failure(NoteWriteFailureReason.InvalidName);

        // The parent must be the user's own active Note - a Note in the Trash isn't a valid
        // parent, and another user's Note looks exactly like a missing one (no leak).
        if (request.ParentId is { } parentId)
        {
            var parentIsActive = await db.Notes.AnyAsync(
                n => n.Id == parentId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
            if (!parentIsActive)
                return NoteWriteResult.Failure(NoteWriteFailureReason.ParentNotFound);
        }

        var now = DateTimeOffset.UtcNow;
        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            ParentId = request.ParentId,
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

        return NoteWriteResult.Success(ToResponse(note, [], await HasDuplicateSiblingNameAsync(note, cancellationToken)));
    }

    public async Task<NoteWriteResult> RenameAsync(Guid userId, Guid noteId, RenameNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return NoteWriteResult.Failure(NoteWriteFailureReason.NotFound);

        var name = NormalizeName(request.Name);
        if (name is null)
            return NoteWriteResult.Failure(NoteWriteFailureReason.InvalidName);

        // Only Name and UpdatedAt change - Content and the AI trigger are untouched, so a
        // rename never counts as a content save.
        note.Name = name;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var categoryIds = await CategoryIdsForAsync(noteId, cancellationToken);
        return NoteWriteResult.Success(ToResponse(note, categoryIds, await HasDuplicateSiblingNameAsync(note, cancellationToken)));
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
            .Where(n => n.UserId == userId && n.DeletedAt != null && (n.DeletionBatchId == null || n.DeletionBatchId == n.Id))
            .OrderByDescending(n => n.DeletedAt)
            .ToListAsync(cancellationToken);

        return await ToResponsesAsync(notes, cancellationToken);
    }

    public async Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return null;

        // Overwrite, not append - Notes have no version history (CONTEXT.md).
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
        // Serializable: the subtree we read and the rows we trash must be one consistent snapshot, so
        // a child created or moved meanwhile can't be left active under a trashed parent. On a clash
        // the database aborts one side and the caller sees a failed request to retry.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var exists = await db.Notes.AnyAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (!exists)
            return false;

        // The Note and every active descendant go to the Trash together, stamped with the
        // deleted Note's Id as the batch. Descendants already in the Trash keep their own
        // batch, so restoring this one later won't pull them back.
        var treeIds = await ActiveSubtreeIdsAsync(userId, noteId, cancellationToken);
        var deletedAt = DateTimeOffset.UtcNow;
        await db.Notes.Where(n => treeIds.Contains(n.Id) && n.DeletedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.DeletedAt, deletedAt)
                .SetProperty(n => n.DeletionBatchId, noteId), cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        foreach (var id in treeIds)
            await eventPublisher.PublishAsync(new NoteDeleted(id, userId, deletedAt), cancellationToken);

        return true;
    }

    public async Task<NoteWriteResult> RestoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        // Serializable for the same reason as DeleteAsync: the parent check and the batch we
        // restore must agree with each other.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt != null, cancellationToken);
        if (note is null)
            return NoteWriteResult.Failure(NoteWriteFailureReason.NotFound);

        // A Note can't come back under a parent that's still in the Trash.
        if (note.ParentId is { } parentId
            && await db.Notes.AnyAsync(n => n.Id == parentId && n.UserId == userId && n.DeletedAt != null, cancellationToken))
            return NoteWriteResult.Failure(NoteWriteFailureReason.ParentInTrash);

        // Everything trashed by the same delete action comes back together. A Note trashed
        // before DeletionBatchId existed has no batch and restores alone.
        var batch = note.DeletionBatchId is { } batchId
            ? await db.Notes.Where(n => n.UserId == userId && n.DeletionBatchId == batchId).ToListAsync(cancellationToken)
            : [note];

        var now = DateTimeOffset.UtcNow;
        var unfiled = new List<(Guid NoteId, Guid OldTopicId)>();
        foreach (var member in batch)
        {
            // Restoring must always succeed rather than fail outright - if the Note's old Topic
            // has since gained another active Note (only possible while this one sat in the
            // Trash), come back unfiled instead of colliding with whoever's there now.
            if (member.TopicId is { } topicId
                && await db.Notes.AnyAsync(n => n.TopicId == topicId && n.DeletedAt == null && n.Id != member.Id, cancellationToken))
            {
                unfiled.Add((member.Id, topicId));
                member.TopicId = null;
            }

            // UpdatedAt stays as it was: a restore isn't an edit, so the Note keeps its place in the list.
            member.DeletedAt = null;
            member.DeletionBatchId = null;
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        foreach (var member in batch)
            await eventPublisher.PublishAsync(new NoteRestored(member.Id, member.UserId, now), cancellationToken);
        // Search keeps its own copy of the Topic, so tell it the Note was unfiled.
        foreach (var (unfiledNoteId, oldTopicId) in unfiled)
            await eventPublisher.PublishAsync(new NoteTopicAssigned(unfiledNoteId, userId, oldTopicId, null, now), cancellationToken);

        var categoryIds = await CategoryIdsForAsync(noteId, cancellationToken);
        return NoteWriteResult.Success(ToResponse(note, categoryIds, await HasDuplicateSiblingNameAsync(note, cancellationToken)));
    }

    public async Task<NoteResponse?> GetByTopicAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.AsNoTracking()
            .SingleOrDefaultAsync(n => n.TopicId == topicId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return null;

        var categoryIds = await CategoryIdsForAsync(note.Id, cancellationToken);
        return ToResponse(note, categoryIds);
    }

    public async Task<UpsertTopicNoteResult> UpsertByTopicAsync(Guid userId, Guid topicId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var topicExists = await db.KnownTopics.AnyAsync(t => t.TopicId == topicId && t.UserId == userId, cancellationToken);
        if (!topicExists)
            return UpsertTopicNoteResult.Failure(UpsertTopicNoteFailureReason.TopicNotFound);

        var note = await db.Notes.SingleOrDefaultAsync(n => n.TopicId == topicId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (note is not null)
        {
            // Overwrite, not append - same as UpdateAsync, Notes have no version history.
            note.Content = request.Content;
            note.UpdatedAt = now;

            var triggersNow = note.AiCategorizationTriggeredAt is null && !string.IsNullOrWhiteSpace(note.Content);
            if (triggersNow)
                note.AiCategorizationTriggeredAt = now;

            await db.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(new NoteUpdated(note.Id, note.UserId, note.Content, now), cancellationToken);
            if (triggersNow)
                await eventPublisher.PublishAsync(new NoteReadyForCategorization(note.Id, note.UserId, note.Content, now), cancellationToken);

            return UpsertTopicNoteResult.Success(await ToResponseWithCategoriesAsync(note, cancellationToken));
        }

        var hasContent = !string.IsNullOrWhiteSpace(request.Content);
        var created = new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Content = request.Content,
            TopicId = topicId,
            CreatedAt = now,
            UpdatedAt = now,
            AiCategorizationTriggeredAt = hasContent ? now : null,
        };

        db.Notes.Add(created);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteCreated(created.Id, created.UserId, created.Content, created.CreatedAt), cancellationToken);
        // The Note is filed under the Topic from birth, not via a separate AssignTopicAsync
        // call - still publish NoteTopicAssigned so OrganizationNoteCountProjectionHandler's
        // Topic.NoteCount stays correct without needing to know about this new creation path.
        await eventPublisher.PublishAsync(new NoteTopicAssigned(created.Id, created.UserId, null, topicId, now), cancellationToken);
        if (hasContent)
            await eventPublisher.PublishAsync(new NoteReadyForCategorization(created.Id, created.UserId, created.Content, now), cancellationToken);

        return UpsertTopicNoteResult.Success(ToResponse(created, []));
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

            // A Topic can have at most one active Note (NoteConfiguration's unique filtered
            // index) - reject rather than silently displacing whatever's already there.
            var topicOccupied = await db.Notes.AnyAsync(
                n => n.TopicId == newTopicId && n.DeletedAt == null && n.Id != noteId, cancellationToken);
            if (topicOccupied)
                return AssignNoteTopicResult.Failure(AssignNoteTopicFailureReason.TopicAlreadyHasNote);
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

    // The Note itself plus every active descendant, found level by level from one flat read
    // of the user's active (Id, ParentId) pairs - no recursive query needed.
    private async Task<List<Guid>> ActiveSubtreeIdsAsync(Guid userId, Guid rootId, CancellationToken cancellationToken)
    {
        var links = await db.Notes.AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt == null && n.ParentId != null)
            .Select(n => new { n.Id, ParentId = n.ParentId!.Value })
            .ToListAsync(cancellationToken);
        var childrenOf = links.ToLookup(l => l.ParentId, l => l.Id);

        var ids = new List<Guid> { rootId };
        for (var i = 0; i < ids.Count; i++)
            ids.AddRange(childrenOf[ids[i]]);
        return ids;
    }

    // Another active Note under the same parent with the same name (ignoring case) - only ever
    // a warning for the user, never a reason to refuse the write.
    private Task<bool> HasDuplicateSiblingNameAsync(Note note, CancellationToken cancellationToken)
    {
        var name = note.Name.ToLower();
        return db.Notes.AsNoTracking().AnyAsync(
            n => n.UserId == note.UserId && n.DeletedAt == null && n.Id != note.Id
                 && n.ParentId == note.ParentId && n.Name.ToLower() == name,
            cancellationToken);
    }

    // Trims the name; null means invalid (empty after trimming, or over the max length).
    private static string? NormalizeName(string? name)
    {
        var trimmed = name?.Trim();
        return string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxNameLength ? null : trimmed;
    }

    private static NoteResponse ToResponse(Note note, IReadOnlyList<Guid> categoryIds, bool hasDuplicateSiblingName = false) => new()
    {
        Id = note.Id,
        Name = note.Name,
        Content = note.Content,
        ParentId = note.ParentId,
        TopicId = note.TopicId,
        CategoryIds = categoryIds,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt,
        DeletedAt = note.DeletedAt,
        HasDuplicateSiblingName = hasDuplicateSiblingName,
    };
}
