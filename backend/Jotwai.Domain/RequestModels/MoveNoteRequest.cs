namespace Jotwai.Domain.RequestModels;

public sealed class MoveNoteRequest
{
    /// <summary>The Note to move under; null moves the Note to the top level.</summary>
    public Guid? ParentId { get; set; }
}
