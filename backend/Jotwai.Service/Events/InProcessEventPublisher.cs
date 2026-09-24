using Jotwai.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Jotwai.Service.Events;

/// <summary>
/// Resolves every registered <see cref="IEventHandler{TEvent}"/> for the published event's
/// concrete type from the current DI scope and awaits them in turn (ADR-0002). No handler
/// registered for an event is not an error - a Module can publish before anyone subscribes.
/// </summary>
public sealed class InProcessEventPublisher(IServiceProvider serviceProvider) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
            await handler.HandleAsync(domainEvent, cancellationToken);
    }
}
