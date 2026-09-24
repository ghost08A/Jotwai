using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum CategoryWriteFailureReason
{
    DuplicateName,
    NotFound,
}

public sealed class CategoryWriteResult
{
    public bool Succeeded { get; init; }
    public CategoryResponse? Category { get; init; }
    public CategoryWriteFailureReason? FailureReason { get; init; }

    public static CategoryWriteResult Success(CategoryResponse category) => new() { Succeeded = true, Category = category };

    public static CategoryWriteResult Failure(CategoryWriteFailureReason reason) => new() { Succeeded = false, FailureReason = reason };
}
