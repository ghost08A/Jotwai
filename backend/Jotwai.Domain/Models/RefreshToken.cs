namespace Jotwai.Domain.Models;

public sealed class RefreshToken
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool Revoked { get; set; }
}
