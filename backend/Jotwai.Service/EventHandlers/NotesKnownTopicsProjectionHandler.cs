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
public sealed class NotesKnownTopicsProjectionHandler(NotesDbContext db) :
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
    }
}
