using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
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
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteCreated(note.Id, note.UserId, note.Content, note.CreatedAt), cancellationToken);

        return ToResponse(note);
    }

    public async Task<NoteResponse?> GetByIdAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.AsNoTracking()
            .SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId, cancellationToken);

        return note is null ? null : ToResponse(note);
    }

    public async Task<IReadOnlyList<NoteResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notes = await db.Notes.AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt == null)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

        return notes.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<NoteResponse>> ListTrashAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notes = await db.Notes.AsNoTracking()
            .Where(n => n.UserId == userId && n.DeletedAt != null)
            .OrderByDescending(n => n.DeletedAt)
            .ToListAsync(cancellationToken);

        return notes.Select(ToResponse).ToList();
    }

    public async Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.DeletedAt == null, cancellationToken);
        if (note is null)
            return null;

        // Overwrite, not append - Notes have no version history (GLOSSARY.md).
        note.Content = request.Content;
        note.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteUpdated(note.Id, note.UserId, note.Content, note.UpdatedAt), cancellationToken);

        return ToResponse(note);
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
            return AssignNoteTopicResult.Success(ToResponse(note));

        note.TopicId = topicId;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new NoteTopicAssigned(note.Id, note.UserId, oldTopicId, topicId, note.UpdatedAt), cancellationToken);

        return AssignNoteTopicResult.Success(ToResponse(note));
    }

    private static NoteResponse ToResponse(Note note) => new()
    {
        Id = note.Id,
        Content = note.Content,
        TopicId = note.TopicId,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt,
        DeletedAt = note.DeletedAt,
    };
}
