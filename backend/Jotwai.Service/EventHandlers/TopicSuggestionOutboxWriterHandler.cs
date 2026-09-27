using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Writes the durable outbox row (ADR-0004) when the Organization Module creates a new
/// Topic - Topic Suggestion (ticket 040) analyzes it in the background from there, the same
/// pattern as AiCategorizationOutboxWriterHandler for Notes.
/// </summary>
public sealed class TopicSuggestionOutboxWriterHandler(AiCategorizationDbContext db) : IEventHandler<TopicCreated>
{
    public async Task HandleAsync(TopicCreated domainEvent, CancellationToken cancellationToken = default)
    {
        db.TopicSuggestionOutboxEntries.Add(new TopicSuggestionOutboxEntry
        {
            Id = Guid.NewGuid(),
            TopicId = domainEvent.TopicId,
            UserId = domainEvent.UserId,
            Name = domainEvent.Name,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
