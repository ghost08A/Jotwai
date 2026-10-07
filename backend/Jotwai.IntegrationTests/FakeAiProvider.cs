using Jotwai.Domain.Enums;
using Jotwai.Domain.Interfaces;

namespace Jotwai.IntegrationTests;

/// <summary>
/// Deterministic stand-in for Gemini in tests - no network call, no real API key needed.
/// Content containing "NOCONFIDENCE" simulates the AI being unsure (ticket 009's threshold
/// then hides it); otherwise it reuses an existing category name mentioned in the content,
/// or proposes a new one, so both branches of the categorization flow are exercisable.
/// </summary>
public sealed class FakeAiProvider : IAiProvider
{
    public Task<IReadOnlyList<AiSuggestedCategory>> SuggestCategoriesAsync(
        string noteContent, IReadOnlyList<string> existingCategoryNames, CancellationToken cancellationToken = default)
    {
        if (noteContent.Contains("NOCONFIDENCE", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<IReadOnlyList<AiSuggestedCategory>>([new AiSuggestedCategory("Barely There", 0.1)]);

        var existingMatch = existingCategoryNames.FirstOrDefault(name => noteContent.Contains(name, StringComparison.OrdinalIgnoreCase));

        IReadOnlyList<AiSuggestedCategory> result = existingMatch is not null
            ? [new AiSuggestedCategory(existingMatch, 0.95)]
            : [new AiSuggestedCategory("Suggested New Category", 0.85)];

        return Task.FromResult(result);
    }

    // Deterministic marker so tests can assert the endpoint round-tripped through the
    // provider without needing to simulate real rewriting behavior.
    public Task<string> PolishNoteContentAsync(string content, CancellationToken cancellationToken = default)
        => Task.FromResult(content + "\n[polished]");

    // Deterministic stand-in for Topic Suggestion (ticket 040): an exact case-insensitive
    // name match is a "Merge" candidate; a name (or, since ticket 044, content) of the form
    // "Sub of <existing name>" is a "Placement" candidate; anything else is "none" (null) -
    // checking content the same way lets tests exercise the "name is a placeholder, content
    // carries the real signal" path added in ticket 044.
    public Task<AiTopicSuggestion?> AnalyzeTopicAsync(
        string newTopicName, string? newTopicContent, IReadOnlyList<AiTopicCandidate> existingTopics, CancellationToken cancellationToken = default)
    {
        var exactMatch = existingTopics.FirstOrDefault(t => string.Equals(t.Name, newTopicName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
            return Task.FromResult<AiTopicSuggestion?>(new AiTopicSuggestion(TopicSuggestionKind.Merge, exactMatch.TopicId, 0.99));

        const string placementPrefix = "Sub of ";
        var placement = TryFindPlacement(newTopicName, placementPrefix, existingTopics)
            ?? TryFindPlacement(newTopicContent ?? string.Empty, placementPrefix, existingTopics);
        if (placement is not null)
            return Task.FromResult<AiTopicSuggestion?>(new AiTopicSuggestion(TopicSuggestionKind.Placement, placement.Value, 0.9));

        return Task.FromResult<AiTopicSuggestion?>(null);
    }

    // Deterministic stand-in for Note Suggestion (Spec 1, ticket 4/7): a name of the form
    // "Sub of <existing note name>" is a Placement (0.9, above the threshold); "Maybe sub of
    // <name>" is a weak one (0.5, below it); anything else is "none" (null).
    public Task<AiNoteSuggestion?> AnalyzeNoteAsync(
        string newNoteName, string newNoteContent, IReadOnlyList<AiNoteCandidate> existingNotes, CancellationToken cancellationToken = default)
    {
        AiNoteSuggestion? Match(string prefix, double confidence)
        {
            if (!newNoteName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var parentName = newNoteName[prefix.Length..];
            var parent = existingNotes.FirstOrDefault(n => string.Equals(n.Name, parentName, StringComparison.OrdinalIgnoreCase));
            return parent is null ? null : new AiNoteSuggestion(NoteSuggestionKind.Placement, parent.NoteId, confidence);
        }

        return Task.FromResult(Match("Sub of ", 0.9) ?? Match("Maybe sub of ", 0.5));
    }

    private static Guid? TryFindPlacement(string text, string placementPrefix, IReadOnlyList<AiTopicCandidate> existingTopics)
    {
        if (!text.StartsWith(placementPrefix, StringComparison.OrdinalIgnoreCase))
            return null;

        var parentName = text[placementPrefix.Length..];
        var parentMatch = existingTopics.FirstOrDefault(t => string.Equals(t.Name, parentName, StringComparison.OrdinalIgnoreCase));
        return parentMatch?.TopicId;
    }
}
