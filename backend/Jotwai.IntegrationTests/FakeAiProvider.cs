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
}
