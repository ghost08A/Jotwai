using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteCreated(Guid NoteId, Guid UserId, string Content, DateTimeOffset CreatedAt) : IDomainEvent;
