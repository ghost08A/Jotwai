namespace Jotwai.Domain.RequestModels;

public sealed class CreateNoteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    /// <summary>The parent Note to create this Note under; null creates a top-level Note.</summary>
    public Guid? ParentId { get; set; }
}
