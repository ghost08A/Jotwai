using System.ComponentModel.DataAnnotations;

namespace Jotwai.Domain.RequestModels;

public sealed class RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
