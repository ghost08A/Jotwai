using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models.Database;
using Jotwai.Service.EventHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Jotwai.ServiceRegistrations.Features;

public static class SearchServiceRegistration
{
    /// <summary>
    /// Registers the Search feature. This is the only thing Web.Jotwai needs to
    /// call to wire up this feature - everything else stays internal to it.
    /// </summary>
    public static IServiceCollection AddSearchFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<SearchDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "search")));

        // ISearchService/SearchService wired by RegisterAutoServices (Web.Jotwai/Program.cs).

        // Search subscribing to the Notes Module's Note lifecycle, to keep its own
        // SearchDocument read model in sync (ADR-0003) - see SearchNoteProjectionHandler.
        services.AddScoped<IEventHandler<NoteCreated>, SearchNoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteUpdated>, SearchNoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteDeleted>, SearchNoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteRestored>, SearchNoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteTopicAssigned>, SearchNoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteCategoryAttached>, SearchNoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteCategoryDetached>, SearchNoteProjectionHandler>();

        // Search subscribing to the Organization Module's Topic hierarchy, to keep its own
        // SearchTopic read model in sync (ADR-0003) - see SearchTopicProjectionHandler.
        services.AddScoped<IEventHandler<TopicCreated>, SearchTopicProjectionHandler>();
        services.AddScoped<IEventHandler<TopicRenamed>, SearchTopicProjectionHandler>();
        services.AddScoped<IEventHandler<TopicDeleted>, SearchTopicProjectionHandler>();

        // Search subscribing to the Organization Module's Categories, to keep its own
        // SearchCategory read model in sync (ADR-0003) - see SearchCategoryProjectionHandler.
        services.AddScoped<IEventHandler<CategoryCreated>, SearchCategoryProjectionHandler>();
        services.AddScoped<IEventHandler<CategoryRenamed>, SearchCategoryProjectionHandler>();
        services.AddScoped<IEventHandler<CategoryDeleted>, SearchCategoryProjectionHandler>();

        return services;
    }
}
