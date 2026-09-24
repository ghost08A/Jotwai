using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record NoteCategoryDetached(Guid NoteId, Guid UserId, Guid CategoryId, DateTimeOffset DetachedAt) : IDomainEvent;
