using Jotwai.Domain.Enums;

namespace Jotwai.Domain.Models;

/// <summary>
/// Note Suggestion (CONTEXT.md, Spec 1 ticket 4/7) - a single candidate suggestion for a newly
/// created Note, resolved once by the user accepting or ignoring it. Never a ranked list: a
/// Note has at most one parent, so there is only ever one candidate.
/// </summary>
public sealed class NoteSuggestion
{
    public Guid Id { get; init; }
    public required Guid NoteId { get; init; }
    public required Guid UserId { get; init; }
    public required NoteSuggestionKind Kind { get; init; }

    /// <summary>The existing Note this Note should become a child of (Kind = Placement).</summary>
    public required Guid TargetNoteId { get; init; }

    public required double Confidence { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
