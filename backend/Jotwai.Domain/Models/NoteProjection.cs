namespace Jotwai.Domain.Models;

/// <summary>
/// Nerd Assistant's own read model (ADR-0003) of the Notes Module's Note hierarchy - needed to
/// give the AI provider the names of every other live Note, and to leave out a new Note's own
/// descendants, for Note Suggestion (Spec 1, ticket 4/7), without querying the `notes` schema
/// directly. Kept in sync via the Note lifecycle events (see NoteProjectionHandler).
/// </summary>
public sealed class NoteProjection
{
    public required Guid NoteId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }

    /// <summary>Set while the Note is in the Trash - Trash Notes are never candidates.</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
