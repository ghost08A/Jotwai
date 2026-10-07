using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum NoteWriteFailureReason
{
    NotFound,
    ParentNotFound,
    CircularMove,
    InvalidName,
    ParentInTrash,
}

public sealed class NoteWriteResult
{
    public bool Succeeded { get; init; }
    public NoteResponse? Note { get; init; }
    public NoteWriteFailureReason? FailureReason { get; init; }

    public static NoteWriteResult Success(NoteResponse note) => new() { Succeeded = true, Note = note };

    public static NoteWriteResult Failure(NoteWriteFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
