using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class SearchTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public SearchTests(JotwaiApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string ValidPassword = "Str0ng!Pass";

    private async Task<HttpClient> AuthenticatedClientAsync(HttpClient? client = null)
    {
        client ??= _client;
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest { Email = UniqueEmail(), Password = ValidPassword });
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    [Fact]
    public async Task Search_matches_note_content_case_insensitively()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "```csharp\nvar x = 1;\n```" });
        await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "unrelated" });

        var results = await client.GetFromJsonAsync<List<SearchResultResponse>>("/api/search?q=VAR X");

        Assert.Single(results!);
        Assert.Contains("var x = 1;", results![0].Content);
    }

    [Fact]
    public async Task Search_matches_the_notes_category_name()
    {
        var client = await AuthenticatedClientAsync();
        var category = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Recipes" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "tagged note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PostAsync($"/api/notes/{note!.Id}/categories/{category!.Id}", null);

        var results = await client.GetFromJsonAsync<List<SearchResultResponse>>("/api/search?q=recipes");

        Assert.Contains(results!, r => r.Id == note.Id);
    }

    [Fact]
    public async Task Search_filtering_by_multiple_categories_requires_every_one_to_match()
    {
        var client = await AuthenticatedClientAsync();
        var work = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var urgent = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Urgent" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();

        var both = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "tagged with both" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PostAsync($"/api/notes/{both!.Id}/categories/{work!.Id}", null);
        await client.PostAsync($"/api/notes/{both.Id}/categories/{urgent!.Id}", null);

        var onlyOne = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "tagged with one" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PostAsync($"/api/notes/{onlyOne!.Id}/categories/{work.Id}", null);

        var results = await client.GetFromJsonAsync<List<SearchResultResponse>>(
            $"/api/search?categoryIds={work.Id}&categoryIds={urgent.Id}");

        Assert.Single(results!);
        Assert.Equal(both.Id, results![0].Id);
    }

    [Fact]
    public async Task Search_filtering_by_topic_includes_descendant_topics()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var child = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Projects", ParentId = parent!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "under child topic" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PutAsJsonAsync($"/api/notes/{note!.Id}/topic", new AssignNoteTopicRequest { TopicId = child!.Id });

        var results = await client.GetFromJsonAsync<List<SearchResultResponse>>($"/api/search?topicId={parent.Id}");

        Assert.Contains(results!, r => r.Id == note.Id);
    }

    [Fact]
    public async Task Search_excludes_trashed_notes_and_includes_them_again_once_restored()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "trashcan searchable" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        await client.DeleteAsync($"/api/notes/{note!.Id}");
        var whileTrashed = await client.GetFromJsonAsync<List<SearchResultResponse>>("/api/search?q=trashcan");
        Assert.DoesNotContain(whileTrashed!, r => r.Id == note.Id);

        await client.PostAsync($"/api/notes/{note.Id}/restore", null);
        var afterRestore = await client.GetFromJsonAsync<List<SearchResultResponse>>("/api/search?q=trashcan");
        Assert.Contains(afterRestore!, r => r.Id == note.Id);
    }

    [Fact]
    public async Task Search_never_returns_another_users_notes()
    {
        var owner = await AuthenticatedClientAsync();
        await owner.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "owner's secret plan" });

        using var otherClient = await AuthenticatedClientAsync(_factory.CreateClient());

        var results = await otherClient.GetFromJsonAsync<List<SearchResultResponse>>("/api/search?q=secret");

        Assert.Empty(results!);
    }

    [Fact]
    public async Task Search_with_no_query_returns_all_active_notes()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "first" });
        await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "second" });

        var results = await client.GetFromJsonAsync<List<SearchResultResponse>>("/api/search");

        Assert.Equal(2, results!.Count);
    }

    [Fact]
    public async Task Search_endpoint_rejects_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/search?q=anything");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
