using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class TopicsTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public TopicsTests(JotwaiApiFactory factory)
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
    public async Task Create_root_topic_succeeds()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var topic = await response.Content.ReadFromJsonAsync<TopicResponse>();
        Assert.Equal("Work", topic!.Name);
        Assert.Null(topic.ParentId);
    }

    [Fact]
    public async Task Create_child_topic_nests_under_parent()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var response = await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Projects", ParentId = parent!.Id });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var child = await response.Content.ReadFromJsonAsync<TopicResponse>();
        Assert.Equal(parent.Id, child!.ParentId);
    }

    [Fact]
    public async Task Create_with_duplicate_sibling_name_case_insensitive_is_rejected()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" });

        var response = await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "work" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_with_same_name_under_different_parents_is_allowed()
    {
        var client = await AuthenticatedClientAsync();
        var parentA = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var parentB = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Personal" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var underA = await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Misc", ParentId = parentA!.Id });
        var underB = await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Misc", ParentId = parentB!.Id });

        Assert.Equal(HttpStatusCode.Created, underA.StatusCode);
        Assert.Equal(HttpStatusCode.Created, underB.StatusCode);
    }

    [Fact]
    public async Task Create_with_nonexistent_parent_returns_bad_request()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Orphan", ParentId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rename_to_an_existing_sibling_name_is_rejected()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" });
        var personal = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Personal" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var response = await client.PutAsJsonAsync($"/api/topics/{personal!.Id}", new RenameTopicRequest { Name = "Work" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_topic_with_child_topics_is_rejected()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Projects", ParentId = parent!.Id });

        var response = await client.DeleteAsync($"/api/topics/{parent.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_empty_topic_succeeds()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Throwaway" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var response = await client.DeleteAsync($"/api/topics/{topic!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_topic_with_a_note_succeeds_and_soft_deletes_the_note_into_trash()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Projects" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "under Projects" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PutAsJsonAsync($"/api/notes/{note!.Id}/topic", new AssignNoteTopicRequest { TopicId = topic!.Id });

        var deleteResponse = await client.DeleteAsync($"/api/topics/{topic.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var trash = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes/trash");
        var trashedNote = trash!.Single(n => n.Id == note.Id);
        Assert.NotNull(trashedNote.DeletedAt);
        Assert.Null(trashedNote.TopicId);

        var restoreResponse = await client.PostAsync($"/api/notes/{note.Id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);
        var restored = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}");
        Assert.Null(restored!.TopicId);
    }

    [Fact]
    public async Task Assigning_a_note_to_a_nonexistent_topic_returns_bad_request()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "loose" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var response = await client.PutAsJsonAsync($"/api/notes/{note!.Id}/topic", new AssignNoteTopicRequest { TopicId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Assigning_a_note_to_another_users_topic_returns_bad_request()
    {
        var owner = await AuthenticatedClientAsync();
        var topic = await (await owner.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Private" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        using var otherClient = await AuthenticatedClientAsync(_factory.CreateClient());
        var note = await (await otherClient.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "mine" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var response = await otherClient.PutAsJsonAsync($"/api/notes/{note!.Id}/topic", new AssignNoteTopicRequest { TopicId = topic!.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Topics_endpoints_reject_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/topics");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
