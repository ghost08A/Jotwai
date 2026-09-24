using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Service.EventHandlers;
using Jotwai.Service.ImplementServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jotwai.Service;

public static class OrganizationServiceRegistration
{
    /// <summary>
    /// Registers the Organization feature. This is the only thing Web.Jotwai needs to
    /// call to wire up this feature - everything else stays internal to it.
    /// </summary>
    public static IServiceCollection AddOrganizationFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<OrganizationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "organization")));

        services.AddScoped<ITopicsService, TopicsService>();

        // Organization subscribing to the Notes Module's Topic-assignment event, to keep
        // its own NoteCount read model in sync (ADR-0003) - see OrganizationNoteCountProjectionHandler.
        services.AddScoped<IEventHandler<NoteTopicAssigned>, OrganizationNoteCountProjectionHandler>();

        return services;
    }
}
