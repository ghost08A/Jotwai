using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps the Search Module's <see cref="SearchDocument"/> read model (ADR-0003) in sync
/// with the Notes Module's Note lifecycle, so Search can match content/Topic/Category
/// substrings (ADR-0006) without querying the `notes` schema directly.
/// </summary>
public sealed class SearchNoteProjectionHandler(SearchDbContext db) :
    IEventHandler<NoteCreated>,
    IEventHandler<NoteUpdated>,
    IEventHandler<NoteRenamed>,
    IEventHandler<NoteParentChanged>,
    IEventHandler<NoteDeleted>,
    IEventHandler<NoteRestored>,
    IEventHandler<NoteTopicAssigned>,
    IEventHandler<NoteCategoryAttached>,
    IEventHandler<NoteCategoryDetached>
{
    public async Task HandleAsync(NoteCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyExists = await db.SearchDocuments.AnyAsync(d => d.NoteId == domainEvent.NoteId, cancellationToken);
        if (alreadyExists)
            return;

        db.SearchDocuments.Add(new SearchDocument
        {
            NoteId = domainEvent.NoteId,
            UserId = domainEvent.UserId,
            Name = domainEvent.Name,
            ParentId = domainEvent.ParentId,
            Content = domainEvent.Content,
            CreatedAt = domainEvent.CreatedAt,
            UpdatedAt = domainEvent.CreatedAt,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(NoteUpdated domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocuments
            .Where(d => d.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Content, domainEvent.Content)
                .SetProperty(d => d.UpdatedAt, domainEvent.UpdatedAt), cancellationToken);
    }

    public async Task HandleAsync(NoteRenamed domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocuments
            .Where(d => d.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Name, domainEvent.Name)
                .SetProperty(d => d.UpdatedAt, domainEvent.UpdatedAt), cancellationToken);
    }

    public async Task HandleAsync(NoteParentChanged domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocuments
            .Where(d => d.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.ParentId, domainEvent.NewParentId)
                .SetProperty(d => d.UpdatedAt, domainEvent.UpdatedAt), cancellationToken);
    }

    public async Task HandleAsync(NoteDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocuments
            .Where(d => d.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.DeletedAt, domainEvent.DeletedAt), cancellationToken);
    }

    public async Task HandleAsync(NoteRestored domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocuments
            .Where(d => d.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.DeletedAt, (DateTimeOffset?)null), cancellationToken);
    }

    public async Task HandleAsync(NoteTopicAssigned domainEvent, CancellationToken cancellationToken = default)
    {
        string? topicName = null;
        if (domainEvent.NewTopicId is { } newTopicId)
        {
            topicName = await db.SearchTopics.AsNoTracking()
                .Where(t => t.TopicId == newTopicId)
                .Select(t => t.Name)
                .SingleOrDefaultAsync(cancellationToken);
        }

        await db.SearchDocuments
            .Where(d => d.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.TopicId, domainEvent.NewTopicId)
                .SetProperty(d => d.TopicName, topicName), cancellationToken);
    }

    public async Task HandleAsync(NoteCategoryAttached domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyAttached = await db.SearchDocumentCategories.AnyAsync(
            dc => dc.NoteId == domainEvent.NoteId && dc.CategoryId == domainEvent.CategoryId, cancellationToken);
        if (alreadyAttached)
            return;

        db.SearchDocumentCategories.Add(new SearchDocumentCategory { NoteId = domainEvent.NoteId, CategoryId = domainEvent.CategoryId });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(NoteCategoryDetached domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocumentCategories
            .Where(dc => dc.NoteId == domainEvent.NoteId && dc.CategoryId == domainEvent.CategoryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
