using Jotwai.Domain.Models;

namespace Jotwai.Domain.Interfaces;

public interface ITokenService
{
    int AccessTokenExpiresInSeconds { get; }

    string GenerateAccessToken(User user);

    RefreshTokenIssuance GenerateRefreshToken();
}
