namespace Jotwai.Domain.Models;

/// <summary>
/// Durable outbox row (ADR-0004) for Topic Suggestion (ticket 040): written transactionally
/// when the Organization Module publishes <see cref="Events.TopicCreated"/>, or on-demand /
/// automatically by <see cref="Interfaces.ITopicSuggestionsService.TriggerAsync"/> (ticket
/// 044), then polled by <c>TopicSuggestionOutboxWorker</c> so a pending analysis survives an
/// app restart.
/// </summary>
public sealed class TopicSuggestionOutboxEntry
{
    public Guid Id { get; init; }
    public required Guid TopicId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }

    /// <summary>The Topic's Note content at the time this entry was written, if any (ticket
    /// 044 - a Topic created blank/"Untitled" has no meaningful name yet, so the analysis
    /// needs the content too). Null on the original at-creation trigger, which fires before
    /// any content exists.</summary>
    public string? Content { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
