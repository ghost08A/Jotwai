namespace Jotwai.Domain.ValidateModel;

public enum AcceptNoteSuggestionFailureReason
{
    SuggestionNotFound,
    TargetNotAvailable,
}

public sealed class AcceptNoteSuggestionResult
{
    public bool Succeeded { get; init; }
    public AcceptNoteSuggestionFailureReason? FailureReason { get; init; }

    public static AcceptNoteSuggestionResult Success() => new() { Succeeded = true };

    public static AcceptNoteSuggestionResult Failure(AcceptNoteSuggestionFailureReason reason) =>
        new() { Succeeded = false, FailureReason = reason };
}
