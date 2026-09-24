namespace Jotwai.Domain.Models;

/// <summary>
/// One ranked candidate within a <see cref="Suggestion"/>. Exactly one of
/// <see cref="CategoryId"/> (an existing Category) or <see cref="NewCategoryName"/> (a name
/// that doesn't exist yet) is set - never both, never neither (ticket 034). A new-name
/// candidate only becomes a real Category row if the user accepts it.
/// </summary>
public sealed class SuggestionCandidate
{
    public Guid Id { get; init; }
    public required Guid SuggestionId { get; init; }
    public Guid? CategoryId { get; init; }
    public string? NewCategoryName { get; init; }
    public required int Rank { get; init; }
    public required double Confidence { get; init; }
}
