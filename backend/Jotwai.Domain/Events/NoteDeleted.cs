using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteDeleted(Guid NoteId, Guid UserId, DateTimeOffset DeletedAt) : IDomainEvent;
