using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps the AI Categorization Module's <see cref="CategoryProjection"/> read model
/// (ADR-0003) in sync with the Organization Module's Categories - needed to give the AI
/// provider the user's existing category names and to match a suggested name against them,
/// without querying the `organization` schema directly.
/// </summary>
public sealed class AiCategorizationCategoryProjectionHandler(AiCategorizationDbContext db) :
    IEventHandler<CategoryCreated>,
    IEventHandler<CategoryRenamed>,
    IEventHandler<CategoryDeleted>
{
    public async Task HandleAsync(CategoryCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.CategoryProjections.AnyAsync(c => c.CategoryId == domainEvent.CategoryId, cancellationToken);
        if (alreadyKnown)
            return;

        db.CategoryProjections.Add(new CategoryProjection { CategoryId = domainEvent.CategoryId, UserId = domainEvent.UserId, Name = domainEvent.Name });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(CategoryRenamed domainEvent, CancellationToken cancellationToken = default)
    {
        await db.CategoryProjections
            .Where(c => c.CategoryId == domainEvent.CategoryId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Name, domainEvent.NewName), cancellationToken);
    }

    public async Task HandleAsync(CategoryDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.CategoryProjections
            .Where(c => c.CategoryId == domainEvent.CategoryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
