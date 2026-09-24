namespace Jotwai.Domain.Models;

public sealed class Note
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Content { get; set; }

    /// <summary>The Organization Module's Topic this Note is filed under, or null. Plain
    /// value only - no cross-schema FK (ADR-0003); validated against Notes' own
    /// <see cref="KnownTopic"/> read model before being set.</summary>
    public Guid? TopicId { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
