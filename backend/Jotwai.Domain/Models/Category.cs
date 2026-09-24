namespace Jotwai.Domain.Models;

public sealed class Category
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
