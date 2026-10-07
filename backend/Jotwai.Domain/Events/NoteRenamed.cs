using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteRenamed(Guid NoteId, Guid UserId, string Name, DateTimeOffset UpdatedAt) : IDomainEvent;
