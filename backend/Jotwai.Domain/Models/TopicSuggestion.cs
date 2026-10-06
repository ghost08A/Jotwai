using Jotwai.Domain.Enums;

namespace Jotwai.Domain.Models;

/// <summary>
/// Topic Suggestion (CONTEXT.md, ticket 040) - a single candidate suggestion for a newly
/// created Topic, resolved by the user accepting or declining it. Unlike Suggestion
/// (Category Suggestion), this is never a ranked list: a Topic has at most one parent, so
/// there's only ever one candidate.
/// </summary>
public sealed class TopicSuggestion
{
    public Guid Id { get; init; }
    public required Guid TopicId { get; init; }
    public required Guid UserId { get; init; }
    public required TopicSuggestionKind Kind { get; init; }

    /// <summary>The existing Topic to merge into (Kind = Merge) or become a child of
    /// (Kind = Placement).</summary>
    public required Guid TargetTopicId { get; init; }

    public required double Confidence { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
