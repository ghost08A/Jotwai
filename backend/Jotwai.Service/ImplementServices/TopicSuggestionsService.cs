using Jotwai.Domain.Enums;
using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Jotwai.Service.ImplementServices;

public sealed class TopicSuggestionsService(
    AiCategorizationDbContext db,
    ITopicsService topicsService,
    IEventPublisher eventPublisher) : ITopicSuggestionsService
{
    public async Task<TopicSuggestionResponse?> GetActiveAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.TopicSuggestions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.TopicId == topicId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return null;

        var targetName = await db.TopicProjections.AsNoTracking()
            .Where(t => t.TopicId == suggestion.TargetTopicId)
            .Select(t => t.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? "(unknown)";

        return ToResponse(suggestion, targetName);
    }

    public async Task<IReadOnlyList<Guid>> ListActiveTopicIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.TopicSuggestions.AsNoTracking()
            .Where(s => s.UserId == userId && s.ResolvedAt == null)
            .Select(s => s.TopicId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IgnoreAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.TopicSuggestions
            .SingleOrDefaultAsync(s => s.TopicId == topicId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return false;

        suggestion.ResolvedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AcceptTopicSuggestionResult> AcceptAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.TopicSuggestions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.TopicId == topicId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return AcceptTopicSuggestionResult.Failure(AcceptTopicSuggestionFailureReason.SuggestionNotFound);

        return suggestion.Kind == TopicSuggestionKind.Merge
            ? await AcceptMergeAsync(suggestion, cancellationToken)
            : await AcceptPlacementAsync(suggestion, userId, cancellationToken);
    }

    // Placement only touches the `organization` schema (ITopicsService.MoveAsync already
    // publishes TopicParentChanged and commits its own change) - resolving the suggestion
    // afterward is a separate, non-transactional write since there's nothing else to keep
    // atomic with it (worst case on a crash between the two: the suggestion just lingers,
    // it doesn't corrupt any data).
    private async Task<AcceptTopicSuggestionResult> AcceptPlacementAsync(TopicSuggestion suggestion, Guid userId, CancellationToken cancellationToken)
    {
        var moveResult = await topicsService.MoveAsync(userId, suggestion.TopicId, suggestion.TargetTopicId, cancellationToken);
        if (!moveResult.Succeeded)
        {
            return moveResult.FailureReason == TopicWriteFailureReason.DuplicateSiblingName
                ? AcceptTopicSuggestionResult.Failure(AcceptTopicSuggestionFailureReason.DuplicateSiblingName)
                : AcceptTopicSuggestionResult.Failure(AcceptTopicSuggestionFailureReason.SuggestionNotFound);
        }

        await db.TopicSuggestions
            .Where(s => s.Id == suggestion.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ResolvedAt, DateTimeOffset.UtcNow), cancellationToken);

        return AcceptTopicSuggestionResult.Success();
    }

    // Cross-schema, must be atomic (organization: delete the Topic + reparent its direct
    // children; notes: reassign every Note filed under it; ai_categorization: resolve the
    // suggestion) - same manual connection/transaction pattern as
    // SuggestionsService.AcceptAsync, for the same reason (ADR-0002's Domain Events are
    // fire-and-forget, not a fit for a true cross-schema transaction).
    private async Task<AcceptTopicSuggestionResult> AcceptMergeAsync(TopicSuggestion suggestion, CancellationToken cancellationToken)
    {
        var connectionString = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("AiCategorizationDbContext has no connection string.");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var organizationTx = new OrganizationDbContext(new DbContextOptionsBuilder<OrganizationDbContext>().UseNpgsql(connection).Options);
        await organizationTx.Database.UseTransactionAsync(transaction, cancellationToken);
        await using var notesTx = new NotesDbContext(new DbContextOptionsBuilder<NotesDbContext>().UseNpgsql(connection).Options);
        await notesTx.Database.UseTransactionAsync(transaction, cancellationToken);
        await using var aiTx = new AiCategorizationDbContext(new DbContextOptionsBuilder<AiCategorizationDbContext>().UseNpgsql(connection).Options);
        await aiTx.Database.UseTransactionAsync(transaction, cancellationToken);

        var doomedTopicId = suggestion.TopicId;
        var survivorTopicId = suggestion.TargetTopicId;

        // A Topic can have at most one active Note (NoteConfiguration's unique filtered
        // index) - if both sides already have one, the blind bulk-reassign below would
        // collide. Reject rather than invent a content-merging policy nobody asked for; the
        // user resolves it manually by copying content into one document first.
        var doomedHasNote = await notesTx.Notes.AsNoTracking().AnyAsync(n => n.TopicId == doomedTopicId && n.DeletedAt == null, cancellationToken);
        var survivorHasNote = await notesTx.Notes.AsNoTracking().AnyAsync(n => n.TopicId == survivorTopicId && n.DeletedAt == null, cancellationToken);
        if (doomedHasNote && survivorHasNote)
        {
            await transaction.RollbackAsync(cancellationToken);
            return AcceptTopicSuggestionResult.Failure(AcceptTopicSuggestionFailureReason.BothTopicsHaveNotes);
        }

        var childTopicIds = await organizationTx.Topics.AsNoTracking()
            .Where(t => t.ParentId == doomedTopicId)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var movedNoteIds = await notesTx.Notes.AsNoTracking()
            .Where(n => n.TopicId == doomedTopicId)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        await organizationTx.Topics
            .Where(t => t.ParentId == doomedTopicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.ParentId, survivorTopicId), cancellationToken);

        await organizationTx.Topics
            .Where(t => t.Id == doomedTopicId)
            .ExecuteDeleteAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        await notesTx.Notes
            .Where(n => n.TopicId == doomedTopicId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(n => n.TopicId, survivorTopicId).SetProperty(n => n.UpdatedAt, now),
                cancellationToken);

        await aiTx.TopicSuggestions
            .Where(s => s.Id == suggestion.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ResolvedAt, now), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        foreach (var childTopicId in childTopicIds)
            await eventPublisher.PublishAsync(new TopicParentChanged(childTopicId, suggestion.UserId, survivorTopicId), cancellationToken);

        foreach (var noteId in movedNoteIds)
            await eventPublisher.PublishAsync(new NoteTopicAssigned(noteId, suggestion.UserId, doomedTopicId, survivorTopicId, now), cancellationToken);

        await eventPublisher.PublishAsync(new TopicDeleted(doomedTopicId, suggestion.UserId), cancellationToken);

        return AcceptTopicSuggestionResult.Success();
    }

    public async Task<bool> TriggerAsync(Guid userId, Guid topicId, string content, CancellationToken cancellationToken = default)
    {
        var topic = await db.TopicProjections.AsNoTracking()
            .SingleOrDefaultAsync(t => t.TopicId == topicId && t.UserId == userId, cancellationToken);
        if (topic is null)
            return false;

        db.TopicSuggestionOutboxEntries.Add(new TopicSuggestionOutboxEntry
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            UserId = userId,
            Name = topic.Name,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static TopicSuggestionResponse ToResponse(TopicSuggestion suggestion, string targetName) => new()
    {
        Id = suggestion.Id,
        TopicId = suggestion.TopicId,
        Kind = suggestion.Kind,
        TargetTopicId = suggestion.TargetTopicId,
        TargetTopicName = targetName,
        Confidence = suggestion.Confidence,
        CreatedAt = suggestion.CreatedAt,
    };
}
