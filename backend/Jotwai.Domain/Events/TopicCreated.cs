using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record TopicCreated(Guid TopicId, Guid UserId, Guid? ParentId, string Name) : IDomainEvent;
