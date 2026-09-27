namespace Jotwai.Domain.Models;

public sealed class Topic
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public Guid? ParentId { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
