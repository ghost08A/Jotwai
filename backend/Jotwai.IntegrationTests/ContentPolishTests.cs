using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class ContentPolishTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public ContentPolishTests(JotwaiApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string ValidPassword = "Str0ng!Pass";

    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = UniqueEmail(), Password = ValidPassword });
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return _client;
    }

    [Fact]
    public async Task Polishing_content_round_trips_through_the_ai_provider()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/content-polish", new PolishContentRequest { Content = "hello" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var polished = await response.Content.ReadFromJsonAsync<PolishContentResponse>();
        Assert.Contains("[polished]", polished!.Content);
    }

    // Code blocks are never sent to the AI provider (ground rule) - FakeAiProvider appends a
    // marker to whatever it receives, so if the code block survives untouched in the final
    // response, it proves ContentPolishService swapped it for a placeholder before the call
    // and restored it verbatim afterward, rather than letting it round-trip through the "AI".
    [Fact]
    public async Task Code_blocks_are_never_sent_to_the_ai_provider()
    {
        var client = await AuthenticatedClientAsync();
        const string codeBlock = "```csharp\nvar x = 1;\n```";
        var content = $"Some text\n\n{codeBlock}\n\nMore text";

        var response = await client.PostAsJsonAsync("/api/content-polish", new PolishContentRequest { Content = content });

        var polished = await response.Content.ReadFromJsonAsync<PolishContentResponse>();
        Assert.Contains(codeBlock, polished!.Content);
        Assert.DoesNotContain("JOTWAI_CODE_BLOCK", polished.Content);
    }

    [Fact]
    public async Task Content_polish_rejects_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.PostAsJsonAsync("/api/content-polish", new PolishContentRequest { Content = "hello" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
