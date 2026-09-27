using Jotwai.Domain.Enums;

namespace Jotwai.Domain.Interfaces;

public sealed record AiSuggestedCategory(string CategoryName, double Confidence);

/// <summary>An existing Topic offered to the AI as a candidate to compare a new Topic's name against.</summary>
public sealed record AiTopicCandidate(Guid TopicId, string Name);

/// <summary>Topic Suggestion's AI verdict (GLOSSARY.md, ticket 040) - null means neither Merge
/// nor Placement applies.</summary>
public sealed record AiTopicSuggestion(TopicSuggestionKind Kind, Guid MatchedTopicId, double Confidence);

/// <summary>
/// The AI Categorization Module's provider abstraction (ADR-0004) - swapping providers
/// later is a configuration change behind this interface, not a rewrite. The provider only
/// proposes category names and a confidence per name; matching a name against the user's
/// existing Categories is the caller's job (it has the read model, the provider doesn't).
/// </summary>
public interface IAiProvider
{
    Task<IReadOnlyList<AiSuggestedCategory>> SuggestCategoriesAsync(
        string noteContent, IReadOnlyList<string> existingCategoryNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Topic Suggestion (GLOSSARY.md, ticket 040) - compares a newly created Topic's name
    /// against every existing Topic the user has, in one call, and decides whether it should
    /// be Merged into one of them or Placed as a subtopic of one of them (never both).
    /// </summary>
    Task<AiTopicSuggestion?> AnalyzeTopicAsync(
        string newTopicName, IReadOnlyList<AiTopicCandidate> existingTopics, CancellationToken cancellationToken = default);
}
