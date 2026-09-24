namespace Jotwai.Domain.ViewModels;

public sealed class SuggestionCandidateResponse
{
    public required Guid Id { get; init; }
    public Guid? CategoryId { get; init; }
    public string? NewCategoryName { get; init; }
    public required int Rank { get; init; }
    public required double Confidence { get; init; }
}
