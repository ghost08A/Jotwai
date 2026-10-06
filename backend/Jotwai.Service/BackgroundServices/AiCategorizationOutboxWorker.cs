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
/// Polls the AI Categorization outbox (ADR-0004) and turns each pending row into a ranked
/// Suggestion via <see cref="IAiProvider"/>. In-process, no external job framework/broker -
/// the outbox table itself is the durability mechanism, so a pending row survives an app
/// restart between "Note saved" and "Suggestion created".
/// </summary>
public sealed class AiCategorizationOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AiCategorizationWorkerOptions> workerOptions,
    ILogger<AiCategorizationOutboxWorker> logger) : BackgroundService
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
                // A failure processing one batch must not kill the worker - the next tick
                // just tries again.
                logger.LogError(ex, "AI Categorization outbox poll failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingEntriesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiCategorizationDbContext>();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        var pending = await db.OutboxEntries
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

    private async Task ProcessEntryAsync(AiCategorizationDbContext db, IAiProvider aiProvider, AiCategorizationOutboxEntry entry, double threshold, CancellationToken cancellationToken)
    {
        try
        {
            var existingCategories = await db.CategoryProjections.AsNoTracking()
                .Where(c => c.UserId == entry.UserId)
                .ToListAsync(cancellationToken);

            var suggested = await aiProvider.SuggestCategoriesAsync(
                entry.Content, existingCategories.Select(c => c.Name).ToList(), cancellationToken);

            // Below the confidence threshold means "don't show anything" (ticket 009) -
            // no Suggestion row at all, not an empty one.
            if (suggested.Count > 0 && suggested[0].Confidence >= threshold)
            {
                // A Note has at most one active Suggestion (CONTEXT.md) - this only ever
                // finds one here via the on-demand re-trigger (INotesService.TriggerCategorizationAsync),
                // since the one-time automatic path can't fire twice for the same Note.
                await db.Suggestions
                    .Where(s => s.NoteId == entry.NoteId && s.ResolvedAt == null)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ResolvedAt, DateTimeOffset.UtcNow), cancellationToken);

                var now = DateTimeOffset.UtcNow;
                var suggestion = new Suggestion { Id = Guid.NewGuid(), NoteId = entry.NoteId, UserId = entry.UserId, CreatedAt = now };
                db.Suggestions.Add(suggestion);

                var rank = 0;
                foreach (var candidate in suggested)
                {
                    var matched = existingCategories.FirstOrDefault(c =>
                        string.Equals(c.Name, candidate.CategoryName, StringComparison.OrdinalIgnoreCase));

                    db.SuggestionCandidates.Add(new SuggestionCandidate
                    {
                        Id = Guid.NewGuid(),
                        SuggestionId = suggestion.Id,
                        CategoryId = matched?.CategoryId,
                        NewCategoryName = matched is null ? candidate.CategoryName : null,
                        Rank = rank++,
                        Confidence = candidate.Confidence,
                    });
                }
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
                logger.LogWarning(ex, "Giving up on outbox entry {EntryId} for note {NoteId} after {Attempts} attempts.", entry.Id, entry.NoteId, entry.Attempts);
                entry.ProcessedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                logger.LogWarning(ex, "Attempt {Attempt} failed for outbox entry {EntryId} for note {NoteId}.", entry.Attempts, entry.Id, entry.NoteId);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<double> GetConfidenceThresholdAsync(AiCategorizationDbContext db, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == AiCategorizationSettingsConfiguration.SingletonId, cancellationToken);

        return settings?.ConfidenceThreshold ?? AiCategorizationSettingsConfiguration.DefaultConfidenceThreshold;
    }
}
