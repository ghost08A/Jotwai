using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps the Search Module's <see cref="SearchTopic"/> read model (ADR-0003) in sync with
/// the Organization Module's Topic hierarchy, so Search can resolve a Topic filter's
/// descendants and match Topic names (ADR-0006) without querying the `organization`
/// schema directly.
/// </summary>
public sealed class SearchTopicProjectionHandler(SearchDbContext db) :
    IEventHandler<TopicCreated>,
    IEventHandler<TopicRenamed>,
    IEventHandler<TopicDeleted>,
    IEventHandler<TopicParentChanged>
{
    public async Task HandleAsync(TopicCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.SearchTopics.AnyAsync(t => t.TopicId == domainEvent.TopicId, cancellationToken);
        if (alreadyKnown)
            return;

        db.SearchTopics.Add(new SearchTopic
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
        await db.SearchTopics
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Name, domainEvent.NewName), cancellationToken);

        // Cascade the rename onto every SearchDocument currently filed under this Topic -
        // TopicName is a denormalized copy (SearchDocument), not a join.
        await db.SearchDocuments
            .Where(d => d.TopicId == domainEvent.TopicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.TopicName, domainEvent.NewName), cancellationToken);
    }

    public async Task HandleAsync(TopicDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchTopics
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task HandleAsync(TopicParentChanged domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchTopics
            .Where(t => t.TopicId == domainEvent.TopicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.ParentId, domainEvent.NewParentId), cancellationToken);
    }
}
