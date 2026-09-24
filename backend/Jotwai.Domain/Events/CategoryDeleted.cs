using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record CategoryDeleted(Guid CategoryId, Guid UserId) : IDomainEvent;
