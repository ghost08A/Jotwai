namespace Jotwai.Domain.Interfaces;

public sealed record AiSuggestedCategory(string CategoryName, double Confidence);

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
}
