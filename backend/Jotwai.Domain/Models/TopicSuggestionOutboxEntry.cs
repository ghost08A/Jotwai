namespace Jotwai.Domain.Models;

/// <summary>
/// Durable outbox row (ADR-0004) for Topic Suggestion (ticket 040): written transactionally
/// when the Organization Module publishes <see cref="Events.TopicCreated"/>, then polled by
/// <c>TopicSuggestionOutboxWorker</c> so a pending analysis survives an app restart.
/// </summary>
public sealed class TopicSuggestionOutboxEntry
{
    public Guid Id { get; init; }
    public required Guid TopicId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
