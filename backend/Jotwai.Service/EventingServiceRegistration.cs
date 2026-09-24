using Jotwai.Domain.Interfaces;
using Jotwai.Service.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Jotwai.Service;

public static class EventingServiceRegistration
{
    /// <summary>
    /// Registers the in-process Domain Event mediator (ADR-0002). Called once for the whole
    /// API - every Module publishes through and subscribes to this same <see cref="IEventPublisher"/>.
    /// </summary>
    public static IServiceCollection AddDomainEventing(this IServiceCollection services)
    {
        services.AddScoped<IEventPublisher, InProcessEventPublisher>();

        return services;
    }
}
