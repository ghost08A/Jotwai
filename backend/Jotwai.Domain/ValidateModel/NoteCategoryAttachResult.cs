using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum NoteCategoryAttachFailureReason
{
    NoteNotFound,
    CategoryNotFound,
}

public sealed class NoteCategoryAttachResult
{
    public bool Succeeded { get; init; }
    public NoteResponse? Note { get; init; }
    public NoteCategoryAttachFailureReason? FailureReason { get; init; }

    public static NoteCategoryAttachResult Success(NoteResponse note) => new() { Succeeded = true, Note = note };

    public static NoteCategoryAttachResult Failure(NoteCategoryAttachFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
