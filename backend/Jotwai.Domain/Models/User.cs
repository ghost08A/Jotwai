namespace Jotwai.Domain.Models;

public sealed class User
{
    public Guid Id { get; init; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
