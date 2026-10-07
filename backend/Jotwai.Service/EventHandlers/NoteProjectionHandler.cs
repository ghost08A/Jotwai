using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps Nerd Assistant's <see cref="NoteProjection"/> read model (ADR-0003) in sync with the
/// Notes Module's Note lifecycle - needed for Note Suggestion (Spec 1, ticket 4/7) to compare a
/// new Note's name against every other live Note without querying the `notes` schema directly.
/// </summary>
public sealed class NoteProjectionHandler(AiCategorizationDbContext db) :
    IEventHandler<NoteCreated>,
    IEventHandler<NoteRenamed>,
    IEventHandler<NoteParentChanged>,
    IEventHandler<NoteDeleted>,
    IEventHandler<NoteRestored>
{
    public async Task HandleAsync(NoteCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var alreadyKnown = await db.NoteProjections.AnyAsync(n => n.NoteId == domainEvent.NoteId, cancellationToken);
        if (alreadyKnown)
            return;

        db.NoteProjections.Add(new NoteProjection
        {
            NoteId = domainEvent.NoteId,
            UserId = domainEvent.UserId,
            Name = domainEvent.Name,
            ParentId = domainEvent.ParentId,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleAsync(NoteRenamed domainEvent, CancellationToken cancellationToken = default)
    {
        await db.NoteProjections
            .Where(n => n.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.Name, domainEvent.Name), cancellationToken);
    }

    public async Task HandleAsync(NoteParentChanged domainEvent, CancellationToken cancellationToken = default)
    {
        await db.NoteProjections
            .Where(n => n.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.ParentId, domainEvent.NewParentId), cancellationToken);
    }

    public async Task HandleAsync(NoteDeleted domainEvent, CancellationToken cancellationToken = default)
    {
        await db.NoteProjections
            .Where(n => n.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.DeletedAt, domainEvent.DeletedAt), cancellationToken);
    }

    public async Task HandleAsync(NoteRestored domainEvent, CancellationToken cancellationToken = default)
    {
        await db.NoteProjections
            .Where(n => n.NoteId == domainEvent.NoteId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.DeletedAt, (DateTimeOffset?)null), cancellationToken);
    }
}
