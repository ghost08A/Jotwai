using System.ComponentModel.DataAnnotations;

namespace Jotwai.Domain.RequestModels;

public sealed class RefreshTokenRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}
