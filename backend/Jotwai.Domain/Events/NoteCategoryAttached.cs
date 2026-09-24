using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteCategoryAttached(Guid NoteId, Guid UserId, Guid CategoryId, DateTimeOffset AttachedAt) : IDomainEvent;
