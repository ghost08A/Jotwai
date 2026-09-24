using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Writes the durable outbox row (ADR-0004) when the Notes Module reports a Note is ready
/// for categorization. Writing it here, from the event handler, keeps it in the same
/// request/scope as the Note save it responds to (ADR-0002's synchronous-by-default model) -
/// the row exists as soon as the save completes, so it survives a restart even if the
/// poller hasn't picked it up yet.
/// </summary>
public sealed class AiCategorizationOutboxWriterHandler(AiCategorizationDbContext db) : IEventHandler<NoteReadyForCategorization>
{
    public async Task HandleAsync(NoteReadyForCategorization domainEvent, CancellationToken cancellationToken = default)
    {
        db.OutboxEntries.Add(new AiCategorizationOutboxEntry
        {
            Id = Guid.NewGuid(),
            NoteId = domainEvent.NoteId,
            UserId = domainEvent.UserId,
            Content = domainEvent.Content,
            CreatedAt = domainEvent.TriggeredAt,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
