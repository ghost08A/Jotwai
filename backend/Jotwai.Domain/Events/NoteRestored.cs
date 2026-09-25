using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteRestored(Guid NoteId, Guid UserId, DateTimeOffset RestoredAt) : IDomainEvent;
