namespace Jotwai.Domain.Models;

/// <summary>
/// Durable outbox row (ADR-0004): written transactionally when the Notes Module publishes
/// <see cref="Events.NoteReadyForCategorization"/>, then polled by
/// <c>AiCategorizationOutboxWorker</c> so a pending categorization survives an app restart.
/// </summary>
public sealed class AiCategorizationOutboxEntry
{
    public Guid Id { get; init; }
    public required Guid NoteId { get; init; }
    public required Guid UserId { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
