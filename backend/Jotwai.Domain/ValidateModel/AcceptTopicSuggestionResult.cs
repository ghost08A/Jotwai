namespace Jotwai.Domain.ValidateModel;

public enum AcceptTopicSuggestionFailureReason
{
    SuggestionNotFound,
    DuplicateSiblingName,
}

public sealed class AcceptTopicSuggestionResult
{
    public bool Succeeded { get; init; }
    public AcceptTopicSuggestionFailureReason? FailureReason { get; init; }

    public static AcceptTopicSuggestionResult Success() => new() { Succeeded = true };

    public static AcceptTopicSuggestionResult Failure(AcceptTopicSuggestionFailureReason reason) =>
        new() { Succeeded = false, FailureReason = reason };
}
