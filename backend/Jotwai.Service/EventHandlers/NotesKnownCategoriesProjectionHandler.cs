using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps the Notes Module's <see cref="KnownCategory"/> read model (ADR-0003) in sync with
/// the Organization Module's Categories, so Notes can validate attaching a Category to a
/// Note without querying the `organization` schema directly. Also cleans up any
/// <see cref="NoteCategory"/> join rows left dangling once a Category is deleted, since
/// Notes owns that join table and Organization has no visibility into it.
/// </summary>
public sealed class NotesKnownCategoriesProjectionHandler(NotesDbContext db) :
    IEventHandler<CategoryCreated>,
    IEventHandler<CategoryDeleted>
{
    public async Task HandleAsync(CategoryCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.KnownCategories.AnyAsync(c => c.CategoryId == domainEvent.CategoryId, cancellationToken);
        if (alreadyKnown)
            return;

        db.KnownCategories.Add(new KnownCategory { CategoryId = domainEvent.CategoryId, UserId = domainEvent.UserId });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(CategoryDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.NoteCategories
            .Where(nc => nc.CategoryId == domainEvent.CategoryId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.KnownCategories
            .Where(c => c.CategoryId == domainEvent.CategoryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
