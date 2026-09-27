using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum TopicWriteFailureReason
{
    ParentNotFound,
    DuplicateSiblingName,
    NotFound,
    CircularParent,
}

public sealed class TopicWriteResult
{
    public bool Succeeded { get; init; }
    public TopicResponse? Topic { get; init; }
    public TopicWriteFailureReason? FailureReason { get; init; }

    public static TopicWriteResult Success(TopicResponse topic) => new() { Succeeded = true, Topic = topic };

    public static TopicWriteResult Failure(TopicWriteFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
