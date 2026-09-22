using System.Net;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class LogoutTests : IClassFixture<JotwaiApiFactory>
{
    private readonly HttpClient _client;

    public LogoutTests(JotwaiApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string ValidPassword = "Str0ng!Pass";

    private async Task<TokenResponse> RegisterAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = ValidPassword });

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private async Task<TokenResponse> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = ValidPassword });

        return (await response.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    [Fact]
    public async Task Logout_revokes_the_given_refresh_token()
    {
        var tokens = await RegisterAsync(UniqueEmail());

        var logout = await _client.PostAsJsonAsync("/api/auth/logout",
            new RefreshTokenRequest { RefreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = tokens.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Logout_on_one_device_does_not_affect_another_devices_active_token()
    {
        var email = UniqueEmail();
        var deviceA = await RegisterAsync(email);
        var deviceB = await LoginAsync(email);

        var logout = await _client.PostAsJsonAsync("/api/auth/logout",
            new RefreshTokenRequest { RefreshToken = deviceA.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refreshA = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = deviceA.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshA.StatusCode);

        var refreshB = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = deviceB.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshB.StatusCode);
    }

    [Fact]
    public async Task Logout_called_twice_with_the_same_token_is_not_an_error()
    {
        var tokens = await RegisterAsync(UniqueEmail());

        var first = await _client.PostAsJsonAsync("/api/auth/logout",
            new RefreshTokenRequest { RefreshToken = tokens.RefreshToken });
        var second = await _client.PostAsJsonAsync("/api/auth/logout",
            new RefreshTokenRequest { RefreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task Logout_with_an_unknown_token_is_not_an_error()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/logout",
            new RefreshTokenRequest { RefreshToken = "not-a-real-refresh-token" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
