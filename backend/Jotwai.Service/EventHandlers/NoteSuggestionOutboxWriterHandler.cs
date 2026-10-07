using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Writes the durable outbox row (ADR-0004) when the Notes Module creates a new Note - Note
/// Suggestion analyzes it in the background from there. Only top-level, named Notes qualify: a
/// Note created under a parent was already placed by the user, and a Note with no name (made by
/// the old Topic flow) has nothing to compare.
/// </summary>
public sealed class NoteSuggestionOutboxWriterHandler(AiCategorizationDbContext db) : IEventHandler<NoteCreated>
{
    public async Task HandleAsync(NoteCreated domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.ParentId is not null || string.IsNullOrWhiteSpace(domainEvent.Name))
            return;

        db.NoteSuggestionOutboxEntries.Add(new NoteSuggestionOutboxEntry
        {
            Id = Guid.NewGuid(),
            NoteId = domainEvent.NoteId,
            UserId = domainEvent.UserId,
            Name = domainEvent.Name,
            Content = domainEvent.Content,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
