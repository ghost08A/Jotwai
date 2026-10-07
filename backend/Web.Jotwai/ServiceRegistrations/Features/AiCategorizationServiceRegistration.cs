using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models.Database;
using Jotwai.Service;
using Jotwai.Service.AiProviders;
using Jotwai.Service.BackgroundServices;
using Jotwai.Service.EventHandlers;
using Jotwai.Service.ImplementServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Web.Jotwai.ServiceRegistrations.Features;

public static class AiCategorizationServiceRegistration
{
    /// <summary>
    /// Registers the AI Categorization feature. This is the only thing Web.Jotwai needs to
    /// call to wire up this feature - everything else stays internal to it.
    /// </summary>
    public static IServiceCollection AddAiCategorizationFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<AiCategorizationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ai_categorization")));

        var geminiOptions = configuration.GetSection("Gemini").Get<GeminiOptions>()
            ?? throw new InvalidOperationException("Missing 'Gemini' configuration section.");

        services.AddSingleton(Options.Create(geminiOptions));
        services.AddHttpClient<IAiProvider, GeminiAiProvider>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        });

        // ISuggestionsService/SuggestionsService wired by RegisterAutoServices
        // (Web.Jotwai/Program.cs).

        // Bound optionally: absent "AiCategorization" config section keeps the 5s default
        // (see AiCategorizationWorkerOptions) - only test setups need a faster poll.
        services.Configure<AiCategorizationWorkerOptions>(configuration.GetSection("AiCategorization"));
        services.AddHostedService<AiCategorizationOutboxWorker>();

        // AI Categorization subscribing to the Notes Module's trigger event, to write its
        // own outbox row (ADR-0004) - see AiCategorizationOutboxWriterHandler.
        services.AddScoped<IEventHandler<NoteReadyForCategorization>, AiCategorizationOutboxWriterHandler>();

        // AI Categorization subscribing to the Organization Module's Category lifecycle, to
        // keep its own CategoryProjection read model in sync (ADR-0003) - see
        // AiCategorizationCategoryProjectionHandler.
        services.AddScoped<IEventHandler<CategoryCreated>, AiCategorizationCategoryProjectionHandler>();
        services.AddScoped<IEventHandler<CategoryRenamed>, AiCategorizationCategoryProjectionHandler>();
        services.AddScoped<IEventHandler<CategoryDeleted>, AiCategorizationCategoryProjectionHandler>();

        // Nerd Assistant subscribing to the Organization Module's Topic hierarchy, to keep
        // its own TopicProjection read model in sync (ADR-0003, ticket 040) - see
        // TopicProjectionHandler.
        services.AddScoped<IEventHandler<TopicCreated>, TopicProjectionHandler>();
        services.AddScoped<IEventHandler<TopicRenamed>, TopicProjectionHandler>();
        services.AddScoped<IEventHandler<TopicDeleted>, TopicProjectionHandler>();
        services.AddScoped<IEventHandler<TopicParentChanged>, TopicProjectionHandler>();

        // Topic Suggestion (ticket 040): writes its own outbox row when a Topic is created,
        // then TopicSuggestionOutboxWorker analyzes it in the background - the Topic
        // analogue of the Note flow above.
        services.AddScoped<IEventHandler<TopicCreated>, TopicSuggestionOutboxWriterHandler>();
        services.AddHostedService<TopicSuggestionOutboxWorker>();

        // ITopicSuggestionsService/TopicSuggestionsService wired by RegisterAutoServices
        // (Web.Jotwai/Program.cs).

        // Nerd Assistant subscribing to the Notes Module's Note lifecycle, to keep its own
        // NoteProjection read model in sync (ADR-0003) - see NoteProjectionHandler.
        services.AddScoped<IEventHandler<NoteCreated>, NoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteRenamed>, NoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteParentChanged>, NoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteDeleted>, NoteProjectionHandler>();
        services.AddScoped<IEventHandler<NoteRestored>, NoteProjectionHandler>();

        // Note Suggestion (Spec 1, ticket 4/7): writes its own outbox row when a top-level Note is
        // created, then NoteSuggestionOutboxWorker analyzes it in the background.
        services.AddScoped<IEventHandler<NoteCreated>, NoteSuggestionOutboxWriterHandler>();
        services.AddHostedService<NoteSuggestionOutboxWorker>();

        // INoteSuggestionsService/NoteSuggestionsService wired by RegisterAutoServices
        // (Web.Jotwai/Program.cs).

        return services;
    }
}
