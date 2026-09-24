using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.EventHandlers;

/// <summary>
/// Keeps each Topic's denormalized <c>NoteCount</c> (ADR-0003 read model) in sync with the
/// Notes Module's Topic assignments, so Organization can enforce "a Topic with Notes can't
/// be deleted" without querying the `notes` schema directly.
/// </summary>
public sealed class OrganizationNoteCountProjectionHandler(OrganizationDbContext db) : IEventHandler<NoteTopicAssigned>
{
    public async Task HandleAsync(NoteTopicAssigned domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.OldTopicId == domainEvent.NewTopicId)
            return;

        if (domainEvent.OldTopicId is { } oldTopicId)
        {
            await db.Topics
                .Where(t => t.Id == oldTopicId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.NoteCount, t => t.NoteCount - 1), cancellationToken);
        }

        if (domainEvent.NewTopicId is { } newTopicId)
        {
            await db.Topics
                .Where(t => t.Id == newTopicId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.NoteCount, t => t.NoteCount + 1), cancellationToken);
        }
    }
}
