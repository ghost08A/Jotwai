using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

/// <summary>
/// Published whenever a Topic's ParentId changes (moved to a new parent, or promoted to
/// top-level) - Topic Suggestion's Placement (ticket 040) and Merge's subtopic reparenting
/// both go through this, and any Module mirroring the Topic hierarchy read-models it here
/// instead of the (rename-only) TopicRenamed event.
/// </summary>
public sealed record TopicParentChanged(Guid TopicId, Guid UserId, Guid? NewParentId) : IDomainEvent;
