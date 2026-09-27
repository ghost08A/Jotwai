using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps Nerd Assistant's <see cref="TopicProjection"/> read model (ADR-0003) in sync with
/// the Organization Module's Topic hierarchy - needed for Topic Suggestion (ticket 040) to
/// compare a newly created Topic's name against every existing one without querying the
/// `organization` schema directly.
/// </summary>
public sealed class TopicProjectionHandler(AiCategorizationDbContext db) :
    IEventHandler<TopicCreated>,
    IEventHandler<TopicRenamed>,
    IEventHandler<TopicDeleted>,
    IEventHandler<TopicParentChanged>
{
    public async Task HandleAsync(TopicCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.TopicProjections.AnyAsync(t => t.TopicId == domainEvent.TopicId, cancellationToken);
        if (alreadyKnown)
            return;

        db.TopicProjections.Add(new TopicProjection
        {
            TopicId = domainEvent.TopicId,
            UserId = domainEvent.UserId,
            ParentId = domainEvent.ParentId,
            Name = domainEvent.Name,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(TopicRenamed domainEvent, CancellationToken cancellationToken = default)
    {
        await db.TopicProjections
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Name, domainEvent.NewName), cancellationToken);
    }

    public async Task HandleAsync(TopicDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.TopicProjections
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task HandleAsync(TopicParentChanged domainEvent, CancellationToken cancellationToken = default)
    {
        await db.TopicProjections
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.ParentId, domainEvent.NewParentId), cancellationToken);
    }
}
