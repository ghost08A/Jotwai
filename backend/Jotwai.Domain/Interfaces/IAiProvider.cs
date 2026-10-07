using Jotwai.Domain.Enums;

namespace Jotwai.Domain.Interfaces;

public sealed record AiSuggestedCategory(string CategoryName, double Confidence);

/// <summary>An existing Topic offered to the AI as a candidate to compare a new Topic's name against.</summary>
public sealed record AiTopicCandidate(Guid TopicId, string Name);

/// <summary>An existing Note offered to the AI as a candidate to compare a new Note against.</summary>
public sealed record AiNoteCandidate(Guid NoteId, string Name);

/// <summary>Note Suggestion's AI verdict (Spec 1, ticket 4/7) - null means no Placement applies.</summary>
public sealed record AiNoteSuggestion(NoteSuggestionKind Kind, Guid MatchedNoteId, double Confidence);

/// <summary>Topic Suggestion's AI verdict (CONTEXT.md, ticket 040) - null means neither Merge
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
    /// Content Polish (CONTEXT.md, ticket 041) - rewrites Markdown for readability. The
    /// caller (ContentPolishService) has already stripped code blocks into opaque
    /// placeholder lines before calling this; the provider is asked to reproduce those
    /// placeholders verbatim.
    /// </summary>
    Task<string> PolishNoteContentAsync(string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Topic Suggestion (CONTEXT.md, ticket 040) - compares a newly created Topic's name
    /// (and, since ticket 044, its Note content so far - a blank/"Untitled" Topic's name
    /// alone carries no signal) against every existing Topic the user has, in one call, and
    /// decides whether it should be Merged into one of them or Placed as a subtopic of one
    /// of them (never both).
    /// </summary>
    Task<AiTopicSuggestion?> AnalyzeTopicAsync(
        string newTopicName, string? newTopicContent, IReadOnlyList<AiTopicCandidate> existingTopics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Note Suggestion (CONTEXT.md, Spec 1 ticket 4/7) - compares a newly created Note's name and
    /// content against the names of the user's other live Notes in one call, and decides whether it
    /// should be Placed as a child of one of them.
    /// </summary>
    Task<AiNoteSuggestion?> AnalyzeNoteAsync(
        string newNoteName, string newNoteContent, IReadOnlyList<AiNoteCandidate> existingNotes, CancellationToken cancellationToken = default);
}
