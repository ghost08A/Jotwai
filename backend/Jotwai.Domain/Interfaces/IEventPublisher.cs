namespace Jotwai.Domain.Interfaces;

/// <summary>
/// Publishes a Domain Event in-process to every registered <see cref="IEventHandler{TEvent}"/>
/// (ADR-0002). Synchronous and in the same scope/transaction as the caller by default -
/// a handler that needs to defer work (e.g. AI Categorization's outbox, ADR-0004) does so itself.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
