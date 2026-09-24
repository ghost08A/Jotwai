using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models.Database;
using Jotwai.Service.EventHandlers;
using Jotwai.Service.ImplementServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Jotwai.ServiceRegistrations.Features;

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

        // INotesService/NotesService wired by RegisterAutoServices (Web.Jotwai/Program.cs).

        // Notes subscribing to the Organization Module's Topic lifecycle, to keep its own
        // KnownTopic read model in sync (ADR-0003) - see NotesKnownTopicsProjectionHandler.
        services.AddScoped<IEventHandler<TopicCreated>, NotesKnownTopicsProjectionHandler>();
        services.AddScoped<IEventHandler<TopicDeleted>, NotesKnownTopicsProjectionHandler>();

        // Notes subscribing to the Organization Module's Category lifecycle, to keep its own
        // KnownCategory read model in sync (ADR-0003) - see NotesKnownCategoriesProjectionHandler.
        services.AddScoped<IEventHandler<CategoryCreated>, NotesKnownCategoriesProjectionHandler>();
        services.AddScoped<IEventHandler<CategoryDeleted>, NotesKnownCategoriesProjectionHandler>();

        return services;
    }
}
