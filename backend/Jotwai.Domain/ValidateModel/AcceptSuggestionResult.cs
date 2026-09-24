using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum AcceptSuggestionFailureReason
{
    SuggestionNotFound,
    InvalidCandidateIds,
}

public sealed class AcceptSuggestionResult
{
    public bool Succeeded { get; init; }
    public NoteResponse? Note { get; init; }
    public AcceptSuggestionFailureReason? FailureReason { get; init; }

    public static AcceptSuggestionResult Success(NoteResponse note) => new() { Succeeded = true, Note = note };

    public static AcceptSuggestionResult Failure(AcceptSuggestionFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
