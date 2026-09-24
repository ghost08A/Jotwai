using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

/// <summary>
/// Published by the Notes Module whenever a Note's Topic assignment changes (including
/// clearing it). The Organization Module subscribes to this to maintain its own
/// denormalized Note count per Topic (ADR-0003) - it needs this to enforce "a Topic with
/// Notes can't be deleted" without querying the `notes` schema directly.
/// </summary>
public sealed record NoteTopicAssigned(Guid NoteId, Guid UserId, Guid? OldTopicId, Guid? NewTopicId, DateTimeOffset AssignedAt) : IDomainEvent;
