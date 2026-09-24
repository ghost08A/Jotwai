using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class NotesTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public NotesTests(JotwaiApiFactory factory)
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
    public async Task Create_without_topic_or_category_saves_immediately()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "hello jotwai" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal("hello jotwai", note!.Content);
        Assert.Null(note.DeletedAt);
    }

    [Fact]
    public async Task List_returns_only_the_current_users_active_notes()
    {
        var owner = await AuthenticatedClientAsync();
        await owner.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "owner's note" });

        using var otherClient = await AuthenticatedClientAsync(_factory.CreateClient());

        var otherList = await otherClient.GetFromJsonAsync<List<NoteResponse>>("/api/notes");

        Assert.DoesNotContain(otherList!, n => n.Content == "owner's note");
    }

    [Fact]
    public async Task Update_overwrites_content_with_no_version_history()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "draft" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var response = await client.PutAsJsonAsync($"/api/notes/{created!.Id}", new UpdateNoteRequest { Content = "final" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal("final", updated!.Content);
    }

    [Fact]
    public async Task Update_of_someone_elses_note_returns_not_found()
    {
        var owner = await AuthenticatedClientAsync();
        var created = await (await owner.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "mine" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        using var otherClient = await AuthenticatedClientAsync(_factory.CreateClient());

        var response = await otherClient.PutAsJsonAsync($"/api/notes/{created!.Id}", new UpdateNoteRequest { Content = "hijacked" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_moves_the_note_to_trash_and_out_of_the_active_list()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "to be deleted" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var deleteResponse = await client.DeleteAsync($"/api/notes/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var activeList = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes");
        Assert.DoesNotContain(activeList!, n => n.Id == created.Id);

        var trash = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes/trash");
        Assert.Contains(trash!, n => n.Id == created.Id);
    }

    [Fact]
    public async Task Restore_brings_a_trashed_note_back_to_the_active_list()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "restore me" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.DeleteAsync($"/api/notes/{created!.Id}");

        var restoreResponse = await client.PostAsync($"/api/notes/{created.Id}/restore", null);
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);

        var activeList = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes");
        Assert.Contains(activeList!, n => n.Id == created.Id && n.DeletedAt == null);
    }

    [Fact]
    public async Task Delete_of_an_already_deleted_note_returns_not_found()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Content = "once only" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.DeleteAsync($"/api/notes/{created!.Id}");

        var secondDelete = await client.DeleteAsync($"/api/notes/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, secondDelete.StatusCode);
    }

    [Fact]
    public async Task Notes_endpoints_reject_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
