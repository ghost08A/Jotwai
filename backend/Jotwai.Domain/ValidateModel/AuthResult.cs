using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.ValidateModel;

public enum AuthFailureReason
{
    EmailAlreadyRegistered,
    PasswordPolicyViolation,
    InvalidCredentials,
    InvalidRefreshToken,
}

public sealed class AuthResult
{
    public bool Succeeded { get; init; }
    public TokenResponse? Tokens { get; init; }
    public AuthFailureReason? FailureReason { get; init; }
    public string? Error { get; init; }

    public static AuthResult Success(TokenResponse tokens) => new() { Succeeded = true, Tokens = tokens };

    public static AuthResult Failure(AuthFailureReason reason, string error) =>
        new() { Succeeded = false, FailureReason = reason, Error = error };
}
