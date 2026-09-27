using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.Enums;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class TopicSuggestionTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public TopicSuggestionTests(JotwaiApiFactory factory)
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
    private static async Task<TopicSuggestionResponse?> WaitForSuggestionAsync(HttpClient client, Guid topicId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/topics/{topicId}/suggestion");
            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<TopicSuggestionResponse>();

            await Task.Delay(200);
        }

        return null;
    }

    [Fact]
    public async Task An_exact_duplicate_name_eventually_produces_a_merge_suggestion()
    {
        var client = await AuthenticatedClientAsync();
        // Two Topics named identically can only coexist under *different* parents - sibling
        // uniqueness (GLOSSARY.md) already blocks the same-parent case outright at create time.
        var existing = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var otherContainer = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Other Container" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var duplicate = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work", ParentId = otherContainer!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var suggestion = await WaitForSuggestionAsync(client, duplicate!.Id);

        Assert.NotNull(suggestion);
        Assert.Equal(TopicSuggestionKind.Merge, suggestion!.Kind);
        Assert.Equal(existing!.Id, suggestion.TargetTopicId);
    }

    [Fact]
    public async Task A_name_shaped_like_a_subtopic_eventually_produces_a_placement_suggestion()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Projects" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var child = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Sub of Projects" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var suggestion = await WaitForSuggestionAsync(client, child!.Id);

        Assert.NotNull(suggestion);
        Assert.Equal(TopicSuggestionKind.Placement, suggestion!.Kind);
        Assert.Equal(parent!.Id, suggestion.TargetTopicId);
    }

    [Fact]
    public async Task Unrelated_topic_names_never_produce_a_suggestion()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Cooking" });
        var other = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Astronomy" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        await Task.Delay(3000);
        var response = await client.GetAsync($"/api/topics/{other!.Id}/suggestion");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Accepting_a_placement_moves_the_topic_under_the_target()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Projects2" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var child = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Sub of Projects2" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var suggestion = await WaitForSuggestionAsync(client, child!.Id);
        Assert.NotNull(suggestion);

        var acceptResponse = await client.PostAsync($"/api/topics/{child.Id}/suggestion/accept", null);
        Assert.Equal(HttpStatusCode.NoContent, acceptResponse.StatusCode);

        var movedChild = await client.GetFromJsonAsync<TopicResponse>($"/api/topics/{child.Id}");
        Assert.Equal(parent!.Id, movedChild!.ParentId);

        var afterAccept = await client.GetAsync($"/api/topics/{child.Id}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, afterAccept.StatusCode);
    }

    [Fact]
    public async Task Accepting_a_merge_deletes_the_duplicate_and_moves_its_notes_and_subtopics()
    {
        var client = await AuthenticatedClientAsync();
        var existing = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Survivor" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var otherContainer = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Other Container 2" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var duplicate = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Survivor", ParentId = otherContainer!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var childOfDuplicate = await (await client.PostAsJsonAsync("/api/topics",
            new CreateTopicRequest { Name = "Child", ParentId = duplicate!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "filed under the duplicate" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PutAsJsonAsync($"/api/notes/{note!.Id}/topic", new AssignNoteTopicRequest { TopicId = duplicate.Id });

        var suggestion = await WaitForSuggestionAsync(client, duplicate.Id);
        Assert.NotNull(suggestion);
        Assert.Equal(TopicSuggestionKind.Merge, suggestion!.Kind);

        var acceptResponse = await client.PostAsync($"/api/topics/{duplicate.Id}/suggestion/accept", null);
        Assert.Equal(HttpStatusCode.NoContent, acceptResponse.StatusCode);

        var deletedTopic = await client.GetAsync($"/api/topics/{duplicate.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedTopic.StatusCode);

        var movedChild = await client.GetFromJsonAsync<TopicResponse>($"/api/topics/{childOfDuplicate!.Id}");
        Assert.Equal(existing!.Id, movedChild!.ParentId);

        var movedNote = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}");
        Assert.Equal(existing.Id, movedNote!.TopicId);
    }

    [Fact]
    public async Task Accepting_a_merge_when_both_topics_already_have_a_note_is_a_conflict()
    {
        var client = await AuthenticatedClientAsync();
        var existing = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Survivor With Note" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var otherContainer = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Other Container 5" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var duplicate = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Survivor With Note", ParentId = otherContainer!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        await client.PutAsJsonAsync($"/api/notes/by-topic/{existing!.Id}", new UpdateNoteRequest { Content = "survivor's own document" });
        await client.PutAsJsonAsync($"/api/notes/by-topic/{duplicate!.Id}", new UpdateNoteRequest { Content = "duplicate's own document" });

        var suggestion = await WaitForSuggestionAsync(client, duplicate.Id);
        Assert.NotNull(suggestion);

        var acceptResponse = await client.PostAsync($"/api/topics/{duplicate.Id}/suggestion/accept", null);

        Assert.Equal(HttpStatusCode.Conflict, acceptResponse.StatusCode);

        // Nothing changed - the duplicate Topic is still there with its own Note intact.
        var stillThere = await client.GetAsync($"/api/topics/{duplicate.Id}");
        Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
        var duplicateNote = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/by-topic/{duplicate.Id}");
        Assert.Equal("duplicate's own document", duplicateNote!.Content);
    }

    [Fact]
    public async Task Ignoring_a_suggestion_resolves_it_without_changing_anything()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Keep" });
        var otherContainer = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Other Container 3" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var duplicate = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Keep", ParentId = otherContainer!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var suggestion = await WaitForSuggestionAsync(client, duplicate!.Id);
        Assert.NotNull(suggestion);

        var ignoreResponse = await client.PostAsync($"/api/topics/{duplicate.Id}/suggestion/ignore", null);
        Assert.Equal(HttpStatusCode.NoContent, ignoreResponse.StatusCode);

        var stillThere = await client.GetAsync($"/api/topics/{duplicate.Id}");
        Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);

        var afterIgnore = await client.GetAsync($"/api/topics/{duplicate.Id}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, afterIgnore.StatusCode);
    }

    [Fact]
    public async Task Listing_active_suggestions_only_includes_unresolved_ones()
    {
        var client = await AuthenticatedClientAsync();
        var existing = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "List Target" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var otherContainer = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Other Container 4" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var duplicate = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "List Target", ParentId = otherContainer!.Id }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        await WaitForSuggestionAsync(client, duplicate!.Id);

        var active = await client.GetFromJsonAsync<List<Guid>>("/api/topics/suggestions");
        Assert.Contains(duplicate.Id, active!);

        await client.PostAsync($"/api/topics/{duplicate.Id}/suggestion/ignore", null);

        var afterIgnore = await client.GetFromJsonAsync<List<Guid>>("/api/topics/suggestions");
        Assert.DoesNotContain(duplicate.Id, afterIgnore!);
    }

    [Fact]
    public async Task Topic_suggestion_endpoints_reject_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync($"/api/topics/{Guid.NewGuid()}/suggestion");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Triggering_with_content_produces_a_suggestion_the_name_alone_would_not()
    {
        var client = await AuthenticatedClientAsync();
        var target = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Placement Target" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        // A name that matches nothing produces no automatic suggestion - the placeholder-name
        // scenario from ticket 044 ("Untitled" carries no signal on its own).
        var blank = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Untitled" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var noAutoSuggestion = await client.GetAsync($"/api/topics/{blank!.Id}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, noAutoSuggestion.StatusCode);

        var triggerResponse = await client.PostAsJsonAsync(
            $"/api/topics/{blank.Id}/suggestion/trigger",
            new TriggerTopicSuggestionRequest { Content = "Sub of Placement Target" });
        Assert.Equal(HttpStatusCode.NoContent, triggerResponse.StatusCode);

        var suggestion = await WaitForSuggestionAsync(client, blank.Id);

        Assert.NotNull(suggestion);
        Assert.Equal(TopicSuggestionKind.Placement, suggestion!.Kind);
        Assert.Equal(target!.Id, suggestion.TargetTopicId);
    }

    [Fact]
    public async Task Triggering_again_resolves_the_previous_suggestion_instead_of_duplicating_it()
    {
        var client = await AuthenticatedClientAsync();
        var firstTarget = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "First Target" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var secondTarget = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Second Target" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var blank = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Untitled" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        await client.PostAsJsonAsync($"/api/topics/{blank!.Id}/suggestion/trigger",
            new TriggerTopicSuggestionRequest { Content = "Sub of First Target" });
        var firstSuggestion = await WaitForSuggestionAsync(client, blank.Id);
        Assert.Equal(firstTarget!.Id, firstSuggestion!.TargetTopicId);

        await client.PostAsJsonAsync($"/api/topics/{blank.Id}/suggestion/trigger",
            new TriggerTopicSuggestionRequest { Content = "Sub of Second Target" });

        // Poll until the target actually changes (not just "any suggestion exists") - proves
        // the stale one got resolved rather than a second unresolved row sitting alongside it
        // (which would otherwise crash GetActiveAsync's SingleOrDefaultAsync).
        var deadline = DateTime.UtcNow.AddSeconds(10);
        TopicSuggestionResponse? secondSuggestion = null;
        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/topics/{blank.Id}/suggestion");
            Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                secondSuggestion = await response.Content.ReadFromJsonAsync<TopicSuggestionResponse>();
                if (secondSuggestion!.TargetTopicId == secondTarget!.Id) break;
            }

            await Task.Delay(200);
        }

        Assert.NotNull(secondSuggestion);
        Assert.Equal(secondTarget!.Id, secondSuggestion!.TargetTopicId);
    }

    [Fact]
    public async Task Triggering_a_nonexistent_topic_returns_not_found()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/topics/{Guid.NewGuid()}/suggestion/trigger",
            new TriggerTopicSuggestionRequest { Content = "anything" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
