namespace Jotwai.Domain.Interfaces;

/// <summary>
/// A Module's reaction to another Module's Domain Event (ADR-0002). Implementations
/// are resolved from DI by <see cref="IEventPublisher"/> - a Module subscribes simply
/// by registering one of these for the event it cares about.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
