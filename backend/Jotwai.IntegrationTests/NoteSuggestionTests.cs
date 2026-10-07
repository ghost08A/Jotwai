using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.Enums;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

// Note Suggestion - Placement only (Spec 1, ticket 4/7). FakeAiProvider proposes a Placement
// for a name of the form "Sub of <existing note name>" (confidence 0.9) and a weak one for
// "Maybe sub of <name>" (0.5, below the threshold).
public sealed class NoteSuggestionTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public NoteSuggestionTests(JotwaiApiFactory factory)
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

    // Empty content on purpose: it keeps these tests from triggering Category AI Categorization.
    private static async Task<NoteResponse> CreateNoteAsync(HttpClient client, string name, Guid? parentId = null)
    {
        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = name, Content = string.Empty, ParentId = parentId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<NoteResponse>())!;
    }

    // The background worker polls every second in tests (JotwaiApiFactory) - this gives it a
    // bounded window instead of a single fixed sleep.
    private static async Task<NoteSuggestionResponse?> WaitForSuggestionAsync(HttpClient client, Guid noteId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/notes/{noteId}/note-suggestion");
            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<NoteSuggestionResponse>();

            await Task.Delay(200);
        }

        return null;
    }

    // Absence can't be waited for directly. The worker handles outbox rows oldest first, so once a
    // later "sentinel" note (one that does get a suggestion) is analyzed, every earlier note has been too.
    private static async Task AssertNoSuggestionOnceLaterNotesAreProcessedAsync(HttpClient client, Guid noteId, string existingNoteName)
    {
        var sentinel = await CreateNoteAsync(client, $"Sub of {existingNoteName}");
        Assert.NotNull(await WaitForSuggestionAsync(client, sentinel.Id));

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/notes/{noteId}/note-suggestion")).StatusCode);
    }

    [Fact]
    public async Task A_new_top_level_note_that_reads_like_a_sub_note_gets_a_placement_suggestion()
    {
        var client = await AuthenticatedClientAsync();
        var cooking = await CreateNoteAsync(client, "Cooking");
        var note = await CreateNoteAsync(client, "Sub of Cooking");

        var suggestion = await WaitForSuggestionAsync(client, note.Id);

        Assert.NotNull(suggestion);
        Assert.Equal(NoteSuggestionKind.Placement, suggestion.Kind);
        Assert.Equal(note.Id, suggestion.NoteId);
        Assert.Equal(cooking.Id, suggestion.TargetNoteId);
        Assert.Equal("Cooking", suggestion.TargetNoteName);
        var activeIds = await client.GetFromJsonAsync<List<Guid>>("/api/notes/note-suggestions");
        Assert.Contains(note.Id, activeIds!);
    }

    [Fact]
    public async Task A_suggestion_below_the_confidence_threshold_is_not_shown()
    {
        var client = await AuthenticatedClientAsync();
        await CreateNoteAsync(client, "Cooking");
        var weak = await CreateNoteAsync(client, "Maybe sub of Cooking");

        await AssertNoSuggestionOnceLaterNotesAreProcessedAsync(client, weak.Id, "Cooking");
    }

    [Fact]
    public async Task Accepting_a_placement_moves_the_note_under_the_target_immediately_and_resolves_it()
    {
        var client = await AuthenticatedClientAsync();
        var cooking = await CreateNoteAsync(client, "Cooking");
        var note = await CreateNoteAsync(client, "Sub of Cooking");
        await WaitForSuggestionAsync(client, note.Id);

        var response = await client.PostAsync($"/api/notes/{note.Id}/note-suggestion/accept", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var moved = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}");
        Assert.Equal(cooking.Id, moved!.ParentId);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/notes/{note.Id}/note-suggestion")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/api/notes/{note.Id}/note-suggestion/accept", null)).StatusCode);
    }

    [Fact]
    public async Task Ignoring_a_placement_resolves_it_and_leaves_the_note_where_it_is()
    {
        var client = await AuthenticatedClientAsync();
        await CreateNoteAsync(client, "Cooking");
        var note = await CreateNoteAsync(client, "Sub of Cooking");
        await WaitForSuggestionAsync(client, note.Id);

        var response = await client.PostAsync($"/api/notes/{note.Id}/note-suggestion/ignore", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null((await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}"))!.ParentId);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/notes/{note.Id}/note-suggestion")).StatusCode);
    }

    [Fact]
    public async Task A_resolved_suggestion_is_not_raised_again_when_the_note_is_renamed()
    {
        var client = await AuthenticatedClientAsync();
        await CreateNoteAsync(client, "Cooking");
        var note = await CreateNoteAsync(client, "Sub of Cooking");
        await WaitForSuggestionAsync(client, note.Id);
        await client.PostAsync($"/api/notes/{note.Id}/note-suggestion/ignore", null);

        await client.PutAsJsonAsync($"/api/notes/{note.Id}/name", new RenameNoteRequest { Name = "Sub of Cooking again" });

        await AssertNoSuggestionOnceLaterNotesAreProcessedAsync(client, note.Id, "Cooking");
    }

    [Fact]
    public async Task A_note_created_under_a_parent_is_not_analyzed_because_the_user_placed_it()
    {
        var client = await AuthenticatedClientAsync();
        await CreateNoteAsync(client, "Cooking");
        var other = await CreateNoteAsync(client, "Other");
        var placedByUser = await CreateNoteAsync(client, "Sub of Cooking", other.Id);

        await AssertNoSuggestionOnceLaterNotesAreProcessedAsync(client, placedByUser.Id, "Cooking");
    }

    [Fact]
    public async Task Notes_in_the_trash_are_not_candidates()
    {
        var client = await AuthenticatedClientAsync();
        var cooking = await CreateNoteAsync(client, "Cooking");
        await client.DeleteAsync($"/api/notes/{cooking.Id}");
        var note = await CreateNoteAsync(client, "Sub of Cooking");
        // A live "Gardening" lets the sentinel below get a suggestion even though "Cooking" is gone.
        await CreateNoteAsync(client, "Gardening");

        await AssertNoSuggestionOnceLaterNotesAreProcessedAsync(client, note.Id, "Gardening");
    }

    [Fact]
    public async Task A_note_is_never_suggested_to_go_under_one_of_its_own_descendants()
    {
        var client = await AuthenticatedClientAsync();
        var note = await CreateNoteAsync(client, "Sub of Own child");
        await CreateNoteAsync(client, "Own child", note.Id);
        await CreateNoteAsync(client, "Gardening");

        await AssertNoSuggestionOnceLaterNotesAreProcessedAsync(client, note.Id, "Gardening");
    }

    [Fact]
    public async Task Another_users_notes_are_not_candidates_and_their_suggestions_are_not_reachable()
    {
        var owner = await AuthenticatedClientAsync();
        await CreateNoteAsync(owner, "Cooking");
        var ownersNote = await CreateNoteAsync(owner, "Sub of Cooking");
        await WaitForSuggestionAsync(owner, ownersNote.Id);

        using var other = await AuthenticatedClientAsync(_factory.CreateClient());
        var othersNote = await CreateNoteAsync(other, "Sub of Cooking");
        await CreateNoteAsync(other, "Gardening");

        await AssertNoSuggestionOnceLaterNotesAreProcessedAsync(other, othersNote.Id, "Gardening");
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/notes/{ownersNote.Id}/note-suggestion")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PostAsync($"/api/notes/{ownersNote.Id}/note-suggestion/accept", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PostAsync($"/api/notes/{ownersNote.Id}/note-suggestion/ignore", null)).StatusCode);
        Assert.DoesNotContain(ownersNote.Id, (await other.GetFromJsonAsync<List<Guid>>("/api/notes/note-suggestions"))!);
        Assert.Null((await owner.GetFromJsonAsync<NoteResponse>($"/api/notes/{ownersNote.Id}"))!.ParentId);
    }

    [Fact]
    public async Task Accepting_when_the_target_has_since_been_trashed_is_a_conflict_and_changes_nothing()
    {
        var client = await AuthenticatedClientAsync();
        var cooking = await CreateNoteAsync(client, "Cooking");
        var note = await CreateNoteAsync(client, "Sub of Cooking");
        await WaitForSuggestionAsync(client, note.Id);
        await client.DeleteAsync($"/api/notes/{cooking.Id}");

        var response = await client.PostAsync($"/api/notes/{note.Id}/note-suggestion/accept", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null((await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}"))!.ParentId);
    }

    [Fact]
    public async Task Note_suggestion_endpoints_reject_requests_without_a_token()
    {
        using var anonymous = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync($"/api/notes/{Guid.NewGuid()}/note-suggestion")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/notes/note-suggestions")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync($"/api/notes/{Guid.NewGuid()}/note-suggestion/accept", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync($"/api/notes/{Guid.NewGuid()}/note-suggestion/ignore", null)).StatusCode);
    }
}
