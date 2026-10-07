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
/// Polls the Note Suggestion outbox (ADR-0004) and turns each pending row into a
/// <see cref="NoteSuggestion"/> via <see cref="IAiProvider"/> - the Note analogue of
/// TopicSuggestionOutboxWorker.
/// </summary>
public sealed class NoteSuggestionOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AiCategorizationWorkerOptions> workerOptions,
    ILogger<NoteSuggestionOutboxWorker> logger) : BackgroundService
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
                logger.LogError(ex, "Note Suggestion outbox poll failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingEntriesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AiCategorizationDbContext>();
        var aiProvider = scope.ServiceProvider.GetRequiredService<IAiProvider>();

        var pending = await db.NoteSuggestionOutboxEntries
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

    private async Task ProcessEntryAsync(AiCategorizationDbContext db, IAiProvider aiProvider, NoteSuggestionOutboxEntry entry, double threshold, CancellationToken cancellationToken)
    {
        try
        {
            var note = await db.NoteProjections.AsNoTracking()
                .SingleOrDefaultAsync(n => n.NoteId == entry.NoteId && n.UserId == entry.UserId, cancellationToken);

            // Nothing to suggest if the Note is gone, in the Trash, or the user has meanwhile filed
            // it under a parent themselves - the system never moves a Note the user placed.
            if (note is not null && note.DeletedAt is null && note.ParentId is null)
            {
                var suggestion = await AnalyzeAsync(db, aiProvider, entry, threshold, cancellationToken);
                if (suggestion is not null)
                    db.NoteSuggestions.Add(suggestion);
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
                logger.LogWarning(ex, "Giving up on Note Suggestion outbox entry {EntryId} for note {NoteId} after {Attempts} attempts.", entry.Id, entry.NoteId, entry.Attempts);
                entry.ProcessedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                logger.LogWarning(ex, "Attempt {Attempt} failed for Note Suggestion outbox entry {EntryId} for note {NoteId}.", entry.Attempts, entry.Id, entry.NoteId);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<NoteSuggestion?> AnalyzeAsync(AiCategorizationDbContext db, IAiProvider aiProvider, NoteSuggestionOutboxEntry entry, double threshold, CancellationToken cancellationToken)
    {
        var liveNotes = await db.NoteProjections.AsNoTracking()
            .Where(n => n.UserId == entry.UserId && n.DeletedAt == null && n.NoteId != entry.NoteId)
            .Select(n => new { n.NoteId, n.ParentId, n.Name })
            .ToListAsync(cancellationToken);

        // A Note can't be placed under its own descendant, so those are never offered to the AI.
        var childrenOf = liveNotes.Where(n => n.ParentId is not null).ToLookup(n => n.ParentId!.Value, n => n.NoteId);
        var descendantIds = new List<Guid> { entry.NoteId };
        for (var i = 0; i < descendantIds.Count; i++)
            descendantIds.AddRange(childrenOf[descendantIds[i]]);

        var candidates = liveNotes
            .Where(n => !descendantIds.Contains(n.NoteId))
            .Select(n => new AiNoteCandidate(n.NoteId, n.Name))
            .ToList();

        var verdict = await aiProvider.AnalyzeNoteAsync(entry.Name, entry.Content, candidates, cancellationToken);

        // Below the confidence threshold means "don't show anything" - no row at all.
        if (verdict is null || verdict.Confidence < threshold)
            return null;

        return new NoteSuggestion
        {
            Id = Guid.NewGuid(),
            NoteId = entry.NoteId,
            UserId = entry.UserId,
            Kind = verdict.Kind,
            TargetNoteId = verdict.MatchedNoteId,
            Confidence = verdict.Confidence,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    // Note Suggestion shares the Topic Suggestion threshold for now (the higher bar CONTEXT.md
    // asks for); the old Topic setting is renamed when Topic is removed in ticket 7/7.
    private static async Task<double> GetConfidenceThresholdAsync(AiCategorizationDbContext db, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == AiCategorizationSettingsConfiguration.SingletonId, cancellationToken);

        return settings?.TopicSuggestionConfidenceThreshold ?? AiCategorizationSettingsConfiguration.DefaultTopicSuggestionConfidenceThreshold;
    }
}
