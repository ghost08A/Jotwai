using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record TopicRenamed(Guid TopicId, Guid UserId, string NewName) : IDomainEvent;
