using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.Models.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jotwai.Service.BackgroundServices;

/// <summary>
/// Polls the Topic Suggestion outbox (ADR-0004, ticket 040) and turns each pending row into
/// a <see cref="TopicSuggestion"/> via <see cref="IAiProvider"/> - the Topic analogue of
/// AiCategorizationOutboxWorker.
/// </summary>
public sealed class TopicSuggestionOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AiCategorizationWorkerOptions> workerOptions,
    ILogger<TopicSuggestionOutboxWorker> logger) : BackgroundService
{
    private const int BatchSize = 10;
    private const int MaxAttempts = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(workerOptions.Value.PollIntervalSeconds));

        do
        {
            try
            {
                await ProcessPendingEntriesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Topic Suggestion outbox poll failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingEntriesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiCategorizationDbContext>();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        var pending = await db.TopicSuggestionOutboxEntries
            .Where(e => e.ProcessedAt == null && e.Attempts < MaxAttempts)
            .OrderBy(e => e.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
            return;

        var threshold = await GetConfidenceThresholdAsync(db, cancellationToken);

        foreach (var entry in pending)
            await ProcessEntryAsync(db, aiProvider, entry, threshold, cancellationToken);
    }

    private async Task ProcessEntryAsync(AiCategorizationDbContext db, IAiProvider aiProvider, TopicSuggestionOutboxEntry entry, double threshold, CancellationToken cancellationToken)
    {
        try
        {
            var candidates = await db.TopicProjections.AsNoTracking()
                .Where(t => t.UserId == entry.UserId && t.TopicId != entry.TopicId)
                .Select(t => new AiTopicCandidate(t.TopicId, t.Name))
                .ToListAsync(cancellationToken);

            var suggestion = await aiProvider.AnalyzeTopicAsync(entry.Name, candidates, cancellationToken);

            // Below the confidence threshold means "don't show anything" (same rule as
            // Category Suggestion, ticket 009) - no row at all, not an empty one.
            if (suggestion is not null && suggestion.Confidence >= threshold)
            {
                db.TopicSuggestions.Add(new TopicSuggestion
                {
                    Id = Guid.NewGuid(),
                    TopicId = entry.TopicId,
                    UserId = entry.UserId,
                    Kind = suggestion.Kind,
                    TargetTopicId = suggestion.MatchedTopicId,
                    Confidence = suggestion.Confidence,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
            }

            entry.ProcessedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            entry.Attempts++;
            entry.LastError = ex.Message;
            if (entry.Attempts >= MaxAttempts)
            {
                logger.LogWarning(ex, "Giving up on Topic Suggestion outbox entry {EntryId} for topic {TopicId} after {Attempts} attempts.", entry.Id, entry.TopicId, entry.Attempts);
                entry.ProcessedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                logger.LogWarning(ex, "Attempt {Attempt} failed for Topic Suggestion outbox entry {EntryId} for topic {TopicId}.", entry.Attempts, entry.Id, entry.TopicId);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<double> GetConfidenceThresholdAsync(AiCategorizationDbContext db, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == AiCategorizationSettingsConfiguration.SingletonId, cancellationToken);

        return settings?.TopicSuggestionConfidenceThreshold ?? AiCategorizationSettingsConfiguration.DefaultTopicSuggestionConfidenceThreshold;
    }
}
