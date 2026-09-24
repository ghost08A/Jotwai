using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteUpdated(Guid NoteId, Guid UserId, string Content, DateTimeOffset UpdatedAt) : IDomainEvent;
