using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Service.EventHandlers;
using Jotwai.Service.ImplementServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jotwai.Service;

public static class NotesServiceRegistration
{
    /// <summary>
    /// Registers the Notes feature. This is the only thing Web.Jotwai needs to
    /// call to wire up this feature - everything else stays internal to it.
    /// </summary>
    public static IServiceCollection AddNotesFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<NotesDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notes")));

        services.AddScoped<INotesService, NotesService>();

        // Notes subscribing to the Organization Module's Topic lifecycle, to keep its own
        // KnownTopic read model in sync (ADR-0003) - see NotesKnownTopicsProjectionHandler.
        services.AddScoped<IEventHandler<TopicCreated>, NotesKnownTopicsProjectionHandler>();
        services.AddScoped<IEventHandler<TopicDeleted>, NotesKnownTopicsProjectionHandler>();

        return services;
    }
}
