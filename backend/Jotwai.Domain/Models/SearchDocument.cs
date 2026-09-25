namespace Jotwai.Domain.Models;

/// <summary>
/// The Search Module's own read model (ADR-0003) of a Note - one row per Note, kept in
/// sync by subscribing to the Notes Module's lifecycle events. Backs substring search
/// (ADR-0006) without querying the `notes`/`organization` schemas directly.
/// </summary>
public sealed class SearchDocument
{
    public required Guid NoteId { get; init; }
    public required Guid UserId { get; init; }
    public required string Content { get; set; }

    /// <summary>Plain value mirroring the Note's current Topic assignment - no cross-schema
    /// FK (ADR-0003).</summary>
    public Guid? TopicId { get; set; }

    /// <summary>Denormalized copy of the assigned Topic's name (GLOSSARY.md "Search" -
    /// Topic name is searchable), kept in sync via <see cref="Events.TopicRenamed"/>.
    /// Not returned in API responses - matching only.</summary>
    public string? TopicName { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Set from <see cref="Events.NoteDeleted"/>, cleared from
    /// <see cref="Events.NoteRestored"/> - a soft-deleted (Trash) Note is excluded from
    /// search results, same as it's excluded from the Notes Module's own active list.</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
