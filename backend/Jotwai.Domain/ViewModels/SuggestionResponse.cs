namespace Jotwai.Domain.ViewModels;

public sealed class SuggestionResponse
{
    public required Guid Id { get; init; }
    public required Guid NoteId { get; init; }
    public required IReadOnlyList<SuggestionCandidateResponse> Candidates { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
