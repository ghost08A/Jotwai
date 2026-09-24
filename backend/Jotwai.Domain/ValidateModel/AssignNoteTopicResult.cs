using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum AssignNoteTopicFailureReason
{
    NoteNotFound,
    TopicNotFound,
}

public sealed class AssignNoteTopicResult
{
    public bool Succeeded { get; init; }
    public NoteResponse? Note { get; init; }
    public AssignNoteTopicFailureReason? FailureReason { get; init; }

    public static AssignNoteTopicResult Success(NoteResponse note) => new() { Succeeded = true, Note = note };

    public static AssignNoteTopicResult Failure(AssignNoteTopicFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
