namespace Jotwai.Domain.RequestModels;

public sealed class CreateTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}
