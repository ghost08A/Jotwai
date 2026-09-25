using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps the Search Module's <see cref="SearchCategory"/> read model (ADR-0003) in sync
/// with the Organization Module's Categories, so Search can match Category names
/// (ADR-0006) without querying the `organization` schema directly. Also cleans up any
/// <see cref="SearchDocumentCategory"/> join rows left dangling once a Category is
/// deleted, mirroring how the Notes Module cleans up its own NoteCategory join
/// (<see cref="NotesKnownCategoriesProjectionHandler"/>) since neither read model is
/// visible to Organization.
/// </summary>
public sealed class SearchCategoryProjectionHandler(SearchDbContext db) :
    IEventHandler<CategoryCreated>,
    IEventHandler<CategoryRenamed>,
    IEventHandler<CategoryDeleted>
{
    public async Task HandleAsync(CategoryCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.SearchCategories.AnyAsync(c => c.CategoryId == domainEvent.CategoryId, cancellationToken);
        if (alreadyKnown)
            return;

        db.SearchCategories.Add(new SearchCategory
        {
            CategoryId = domainEvent.CategoryId,
            UserId = domainEvent.UserId,
            Name = domainEvent.Name,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(CategoryRenamed domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchCategories
            .Where(c => c.CategoryId == domainEvent.CategoryId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Name, domainEvent.NewName), cancellationToken);
    }

    public async Task HandleAsync(CategoryDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.SearchDocumentCategories
            .Where(dc => dc.CategoryId == domainEvent.CategoryId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.SearchCategories
            .Where(c => c.CategoryId == domainEvent.CategoryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
