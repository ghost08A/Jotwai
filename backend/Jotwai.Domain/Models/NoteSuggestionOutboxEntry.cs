namespace Jotwai.Domain.Models;

/// <summary>
/// Durable outbox row (ADR-0004) for Note Suggestion: written when the Notes Module publishes
/// <see cref="Events.NoteCreated"/> for a top-level Note, then polled by
/// <c>NoteSuggestionOutboxWorker</c> so a pending analysis survives an app restart. Holds the
/// Note's name and content as they were at creation - the one automatic analysis looks at those.
/// </summary>
public sealed class NoteSuggestionOutboxEntry
{
    public Guid Id { get; init; }
    public required Guid NoteId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
