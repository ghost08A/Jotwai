using System.Net;
using System.Net.Http.Json;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;
using Jotwai.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jotwai.IntegrationTests;

public sealed class TokenRefreshTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public TokenRefreshTests(JotwaiApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string ValidPassword = "Str0ng!Pass";

    private async Task<TokenResponse> RegisterAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = UniqueEmail(), Password = ValidPassword });

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private async Task ExpireRefreshTokenAsync(string rawRefreshToken)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var tokenHash = tokenService.HashRefreshToken(rawRefreshToken);
        var entity = await db.RefreshTokens.SingleAsync(t => t.TokenHash == tokenHash);
        entity.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Refresh_with_valid_token_returns_new_token_pair_and_rotates_the_old_one()
    {
        var original = await RegisterAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = original.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rotated = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(rotated);
        Assert.False(string.IsNullOrWhiteSpace(rotated!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(rotated.RefreshToken));
        Assert.NotEqual(original.RefreshToken, rotated.RefreshToken);

        // The old refresh token must be unusable immediately after rotation.
        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = original.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Concurrent_refresh_with_the_same_token_only_succeeds_once()
    {
        var original = await RegisterAsync();

        var first = _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = original.RefreshToken });
        var second = _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = original.RefreshToken });

        var responses = await Task.WhenAll(first, second);

        var succeeded = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        var rejected = responses.Count(r => r.StatusCode == HttpStatusCode.Unauthorized);

        Assert.Equal(1, succeeded);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public async Task Refresh_with_unknown_token_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = "not-a-real-refresh-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_with_expired_token_is_rejected()
    {
        var original = await RegisterAsync();
        await ExpireRefreshTokenAsync(original.RefreshToken);

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = original.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
