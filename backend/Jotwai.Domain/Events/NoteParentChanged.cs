using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteParentChanged(Guid NoteId, Guid UserId, Guid? NewParentId, DateTimeOffset UpdatedAt) : IDomainEvent;
