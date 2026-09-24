using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record CategoryCreated(Guid CategoryId, Guid UserId, string Name) : IDomainEvent;
