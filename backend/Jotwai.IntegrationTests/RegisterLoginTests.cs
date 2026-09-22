using System.Net;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class RegisterLoginTests : IClassFixture<JotwaiApiFactory>
{
    private readonly HttpClient _client;

    public RegisterLoginTests(JotwaiApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string ValidPassword = "Str0ng!Pass";

    [Fact]
    public async Task Register_with_new_email_returns_tokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = UniqueEmail(), Password = ValidPassword });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        Assert.True(tokens.ExpiresIn > 0);
    }

    [Fact]
    public async Task Register_with_duplicate_email_returns_conflict_and_does_not_affect_existing_account()
    {
        var email = UniqueEmail();

        var first = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = ValidPassword });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = "SomeOther1!Pass" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        // The original account must still work exactly as before - proves the
        // duplicate attempt didn't create or overwrite anything.
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = ValidPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Register_with_weak_password_returns_validation_error()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = UniqueEmail(), Password = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_correct_credentials_returns_tokens()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = ValidPassword });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = ValidPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
    }

    [Fact]
    public async Task Login_with_wrong_password_and_unknown_email_return_the_same_generic_error()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = email, Password = ValidPassword });

        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = "TotallyWrong1!" });
        var unknownEmail = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = UniqueEmail(), Password = ValidPassword });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);

        var wrongPasswordBody = await wrongPassword.Content.ReadAsStringAsync();
        var unknownEmailBody = await unknownEmail.Content.ReadAsStringAsync();
        Assert.Equal(wrongPasswordBody, unknownEmailBody);
    }
}
