using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class AiCategorizationTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public AiCategorizationTests(JotwaiApiFactory factory)
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

    // FakeAiProvider is deterministic and the worker polls every 1s in tests (JotwaiApiFactory) -
    // this just gives the background poll a bounded window instead of a single fixed sleep.
    private static async Task<SuggestionResponse?> WaitForSuggestionAsync(HttpClient client, Guid noteId, Guid? excludingId = null)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/notes/{noteId}/suggestion");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var suggestion = await response.Content.ReadFromJsonAsync<SuggestionResponse>();
                if (excludingId is null || suggestion!.Id != excludingId)
                    return suggestion;
            }

            await Task.Delay(200);
        }

        return null;
    }

    [Fact]
    public async Task Creating_a_note_with_content_eventually_produces_a_suggestion()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "Plan the quarterly roadmap" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var suggestion = await WaitForSuggestionAsync(client, note!.Id);

        Assert.NotNull(suggestion);
        Assert.NotEmpty(suggestion!.Candidates);
        Assert.Equal(note.Id, suggestion.NoteId);
    }

    [Fact]
    public async Task Low_confidence_never_produces_a_visible_suggestion()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "NOCONFIDENCE unclear scribble" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        // Give the (fast, test-only) poller several ticks to have processed it, then assert
        // it's still absent - proving suppression, not just that we checked too early.
        await Task.Delay(3000);
        var response = await client.GetAsync($"/api/notes/{note!.Id}/suggestion");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Manual_trigger_produces_a_fresh_suggestion_and_resolves_the_old_one()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "Plan the quarterly roadmap" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var firstSuggestion = await WaitForSuggestionAsync(client, note!.Id);
        Assert.NotNull(firstSuggestion);

        var triggerResponse = await client.PostAsync($"/api/notes/{note.Id}/suggestion/trigger", null);
        Assert.Equal(HttpStatusCode.Accepted, triggerResponse.StatusCode);

        var secondSuggestion = await WaitForSuggestionAsync(client, note.Id, excludingId: firstSuggestion!.Id);

        Assert.NotNull(secondSuggestion);
        Assert.NotEqual(firstSuggestion.Id, secondSuggestion!.Id);
    }

    [Fact]
    public async Task Editing_a_note_never_triggers_a_second_categorization()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "first real content" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var firstSuggestion = await WaitForSuggestionAsync(client, note!.Id);
        Assert.NotNull(firstSuggestion);

        await client.PutAsJsonAsync($"/api/notes/{note.Id}", new UpdateNoteRequest { Content = "completely different content now" });

        // Give the (fast, test-only) poller several ticks to have (not) reacted, then assert
        // the active Suggestion is still the exact same one from the original save - proving
        // the edit never queued a second categorization (User Ownership, ticket 025).
        await Task.Delay(3000);
        var afterEditResponse = await client.GetAsync($"/api/notes/{note.Id}/suggestion");
        Assert.Equal(HttpStatusCode.OK, afterEditResponse.StatusCode);
        var afterEdit = await afterEditResponse.Content.ReadFromJsonAsync<SuggestionResponse>();
        Assert.Equal(firstSuggestion!.Id, afterEdit!.Id);
    }

    [Fact]
    public async Task Accepting_candidates_atomically_creates_categories_and_attaches_them()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "Plan the quarterly roadmap" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var suggestion = await WaitForSuggestionAsync(client, note!.Id);
        Assert.NotNull(suggestion);

        var acceptResponse = await client.PostAsJsonAsync($"/api/notes/{note.Id}/suggestion/accept",
            new AcceptSuggestionRequest { CandidateIds = suggestion!.Candidates.Select(c => c.Id).ToList() });

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);
        var updatedNote = await acceptResponse.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal(suggestion.Candidates.Count, updatedNote!.CategoryIds.Count);

        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        Assert.Contains(categories!, c => c.Name == "Suggested New Category");

        // Resolved - the Suggestion is gone either way (GLOSSARY.md).
        var afterAccept = await client.GetAsync($"/api/notes/{note.Id}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, afterAccept.StatusCode);
    }

    [Fact]
    public async Task Ignoring_a_suggestion_resolves_it_without_attaching_anything()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "Plan the quarterly roadmap" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var suggestion = await WaitForSuggestionAsync(client, note!.Id);
        Assert.NotNull(suggestion);

        var ignoreResponse = await client.PostAsync($"/api/notes/{note.Id}/suggestion/ignore", null);
        Assert.Equal(HttpStatusCode.NoContent, ignoreResponse.StatusCode);

        var refreshedNote = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}");
        Assert.Empty(refreshedNote!.CategoryIds);

        var afterIgnore = await client.GetAsync($"/api/notes/{note.Id}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, afterIgnore.StatusCode);
    }

    [Fact]
    public async Task Accepting_a_suggestion_that_matches_an_existing_category_reuses_it()
    {
        var client = await AuthenticatedClientAsync();
        var existing = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Roadmap" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();

        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "This mentions Roadmap directly" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var suggestion = await WaitForSuggestionAsync(client, note!.Id);
        Assert.NotNull(suggestion);
        Assert.Contains(suggestion!.Candidates, c => c.CategoryId == existing!.Id);

        var acceptResponse = await client.PostAsJsonAsync($"/api/notes/{note.Id}/suggestion/accept",
            new AcceptSuggestionRequest { CandidateIds = [suggestion.Candidates.Single(c => c.CategoryId == existing!.Id).Id] });
        var updatedNote = await acceptResponse.Content.ReadFromJsonAsync<NoteResponse>();

        Assert.Contains(existing!.Id, updatedNote!.CategoryIds);

        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        Assert.Single(categories!, c => c.Name == "Roadmap");
    }

    [Fact]
    public async Task Accepting_with_an_invalid_candidate_id_fails_the_whole_request()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "Plan the quarterly roadmap" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var suggestion = await WaitForSuggestionAsync(client, note!.Id);
        Assert.NotNull(suggestion);

        var response = await client.PostAsJsonAsync($"/api/notes/{note.Id}/suggestion/accept",
            new AcceptSuggestionRequest { CandidateIds = [Guid.NewGuid()] });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var refreshedNote = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}");
        Assert.Empty(refreshedNote!.CategoryIds);
    }

    [Fact]
    public async Task Suggestion_endpoints_reject_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync($"/api/notes/{Guid.NewGuid()}/suggestion");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
