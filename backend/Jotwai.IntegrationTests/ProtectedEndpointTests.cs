using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;
using Jotwai.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Jotwai.IntegrationTests;

public sealed class ProtectedEndpointTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public ProtectedEndpointTests(JotwaiApiFactory factory)
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

    private static Guid ExtractUserId(string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return Guid.Parse(jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    private string BuildAccessToken(string subject, DateTime expires)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, subject) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Me_with_valid_access_token_returns_the_current_users_id()
    {
        var tokens = await RegisterAsync();
        var expectedUserId = ExtractUserId(tokens.AccessToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.Equal(expectedUserId, body!.UserId);
    }

    [Fact]
    public async Task Me_with_no_token_is_rejected()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_expired_access_token_is_rejected()
    {
        var expiredToken = BuildAccessToken(Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(-5));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_malformed_token_is_rejected()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-jwt");
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_a_non_guid_subject_claim_fails_closed_instead_of_erroring()
    {
        // A validly signed, unexpired token whose "sub" isn't a Guid - not something the
        // current issuer produces, but the endpoint must reject it (401), not crash (500).
        var token = BuildAccessToken("not-a-guid", DateTime.UtcNow.AddMinutes(15));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
