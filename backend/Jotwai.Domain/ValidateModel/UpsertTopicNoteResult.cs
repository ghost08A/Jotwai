using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum UpsertTopicNoteFailureReason
{
    TopicNotFound,
}

public sealed class UpsertTopicNoteResult
{
    public bool Succeeded { get; init; }
    public NoteResponse? Note { get; init; }
    public UpsertTopicNoteFailureReason? FailureReason { get; init; }

    public static UpsertTopicNoteResult Success(NoteResponse note) => new() { Succeeded = true, Note = note };

    public static UpsertTopicNoteResult Failure(UpsertTopicNoteFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
