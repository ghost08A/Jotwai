namespace Jotwai.Domain.Models;

public sealed class Note
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Content { get; set; }

    /// <summary>The Note's own name, kept apart from <see cref="Content"/> (ADR-0013). Empty
    /// for Notes created before the Topic/Note merge reaches them (the by-topic flow).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The parent Note this Note sits under, or null for a top-level Note. Same
    /// schema, so an ordinary FK (unlike <see cref="TopicId"/>).</summary>
    public Guid? ParentId { get; set; }

    /// <summary>The Organization Module's Topic this Note is filed under, or null. Plain
    /// value only - no cross-schema FK (ADR-0003); validated against Notes' own
    /// <see cref="KnownTopic"/> read model before being set.</summary>
    public Guid? TopicId { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Set the first time this Note is saved with non-empty Content - never again
    /// after that (User Ownership, ticket 025). Owned by Notes itself (ticket 033) so it can
    /// decide, on its own, whether to publish <see cref="Events.NoteReadyForCategorization"/>.</summary>
    public DateTimeOffset? AiCategorizationTriggeredAt { get; set; }
}
