using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record CategoryRenamed(Guid CategoryId, Guid UserId, string NewName) : IDomainEvent;
