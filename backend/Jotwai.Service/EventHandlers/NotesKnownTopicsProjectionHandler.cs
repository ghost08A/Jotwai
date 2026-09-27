using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps the Notes Module's <see cref="KnownTopic"/> read model (ADR-0003) in sync with
/// the Organization Module's Topics, so Notes can validate a Note's Topic assignment
/// without querying the `organization` schema directly.
/// </summary>
public sealed class NotesKnownTopicsProjectionHandler(NotesDbContext db, IEventPublisher eventPublisher) :
    IEventHandler<TopicCreated>,
    IEventHandler<TopicDeleted>
{
    public async Task HandleAsync(TopicCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.KnownTopics.AnyAsync(t => t.TopicId == domainEvent.TopicId, cancellationToken);
        if (alreadyKnown)
            return;

        db.KnownTopics.Add(new KnownTopic { TopicId = domainEvent.TopicId, UserId = domainEvent.UserId });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(TopicDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.KnownTopics
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteDeleteAsync(cancellationToken);

        // The Topic's own document (at most one active Note - NoteConfiguration's unique
        // filtered index) goes with it: soft-delete into Trash and unfile it, rather than
        // leaving it dangling on a TopicId that no longer exists. Deleting a Topic no longer
        // requires emptying its Note first (ticket - "1 Topic = continuous document").
        var orphanedNoteIds = await db.Notes.AsNoTracking()
            .Where(n => n.TopicId == domainEvent.TopicId && n.DeletedAt == null)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (orphanedNoteIds.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        await db.Notes
            .Where(n => orphanedNoteIds.Contains(n.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(n => n.DeletedAt, now).SetProperty(n => n.TopicId, (Guid?)null),
                cancellationToken);

        foreach (var noteId in orphanedNoteIds)
            await eventPublisher.PublishAsync(new NoteDeleted(noteId, domainEvent.UserId, now), cancellationToken);
    }
}
