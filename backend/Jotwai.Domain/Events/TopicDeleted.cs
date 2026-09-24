using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

public sealed record TopicDeleted(Guid TopicId, Guid UserId) : IDomainEvent;
