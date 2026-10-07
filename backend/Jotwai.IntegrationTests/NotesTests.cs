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

        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "hello jotwai" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal("hello jotwai", note!.Content);
        Assert.Null(note.DeletedAt);
    }

    [Fact]
    public async Task List_returns_only_the_current_users_active_notes()
    {
        var owner = await AuthenticatedClientAsync();
        await owner.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "owner's note" });

        using var otherClient = await AuthenticatedClientAsync(_factory.CreateClient());

        var otherList = await otherClient.GetFromJsonAsync<List<NoteResponse>>("/api/notes");

        Assert.DoesNotContain(otherList!, n => n.Content == "owner's note");
    }

    [Fact]
    public async Task Update_overwrites_content_with_no_version_history()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "draft" }))
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
        var created = await (await owner.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "mine" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        using var otherClient = await AuthenticatedClientAsync(_factory.CreateClient());

        var response = await otherClient.PutAsJsonAsync($"/api/notes/{created!.Id}", new UpdateNoteRequest { Content = "hijacked" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_moves_the_note_to_trash_and_out_of_the_active_list()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "to be deleted" }))
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
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "restore me" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.DeleteAsync($"/api/notes/{created!.Id}");

        var restoreResponse = await client.PostAsync($"/api/notes/{created.Id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        var activeList = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes");
        Assert.Contains(activeList!, n => n.Id == created.Id && n.DeletedAt == null);
    }

    [Fact]
    public async Task Delete_of_an_already_deleted_note_returns_not_found()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "once only" }))
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

    [Fact]
    public async Task Assigning_a_note_to_a_topic_that_already_has_one_is_a_conflict()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Occupied" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var occupant = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "already here" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PutAsJsonAsync($"/api/notes/{occupant!.Id}/topic", new AssignNoteTopicRequest { TopicId = topic!.Id });

        var challenger = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "trying to move in" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        var response = await client.PutAsJsonAsync($"/api/notes/{challenger!.Id}/topic", new AssignNoteTopicRequest { TopicId = topic.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetByTopic_returns_not_found_when_the_topic_has_no_note_yet()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Empty" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var response = await client.GetAsync($"/api/notes/by-topic/{topic!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpsertByTopic_creates_the_topics_note_on_first_call_and_overwrites_on_the_next()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Journal" }))
            .Content.ReadFromJsonAsync<TopicResponse>();

        var firstResponse = await client.PutAsJsonAsync($"/api/notes/by-topic/{topic!.Id}", new UpdateNoteRequest { Content = "first draft" });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var created = await firstResponse.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal("first draft", created!.Content);
        Assert.Equal(topic.Id, created.TopicId);

        var secondResponse = await client.PutAsJsonAsync($"/api/notes/by-topic/{topic.Id}", new UpdateNoteRequest { Content = "keeps writing" });
        var updated = await secondResponse.Content.ReadFromJsonAsync<NoteResponse>();

        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal("keeps writing", updated.Content);

        var allNotes = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes");
        Assert.Single(allNotes!, n => n.TopicId == topic.Id);
    }

    [Fact]
    public async Task Restoring_a_trashed_note_unfiles_it_if_its_old_topic_gained_another_note_meanwhile()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Reused" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var original = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "original occupant" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PutAsJsonAsync($"/api/notes/{original!.Id}/topic", new AssignNoteTopicRequest { TopicId = topic!.Id });
        await client.DeleteAsync($"/api/notes/{original.Id}");

        // While `original` sat in the Trash, the Topic got a new document.
        await client.PutAsJsonAsync($"/api/notes/by-topic/{topic.Id}", new UpdateNoteRequest { Content = "new occupant" });

        var restoreResponse = await client.PostAsync($"/api/notes/{original.Id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        var restored = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{original.Id}");
        Assert.Null(restored!.TopicId);
    }

    [Fact]
    public async Task UpsertByTopic_for_a_nonexistent_topic_returns_not_found()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync($"/api/notes/by-topic/{Guid.NewGuid()}", new UpdateNoteRequest { Content = "anything" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Empty content on purpose: it keeps these hierarchy tests from triggering AI Categorization.
    private static async Task<NoteResponse> CreateNamedNoteAsync(HttpClient client, string name, Guid? parentId = null)
    {
        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = name, Content = string.Empty, ParentId = parentId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<NoteResponse>())!;
    }

    [Fact]
    public async Task Create_a_top_level_note_with_a_name_reads_back_with_no_parent()
    {
        var client = await AuthenticatedClientAsync();

        var created = await CreateNamedNoteAsync(client, "Projects");

        var fetched = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{created.Id}");
        Assert.Equal("Projects", fetched!.Name);
        Assert.Null(fetched.ParentId);
    }

    [Fact]
    public async Task Create_a_child_note_reads_back_with_its_parent_in_get_and_list()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await CreateNamedNoteAsync(client, "Parent");

        var child = await CreateNamedNoteAsync(client, "Child", parent.Id);

        var fetched = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{child.Id}");
        Assert.Equal(parent.Id, fetched!.ParentId);
        var list = await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes");
        Assert.Equal(parent.Id, list!.Single(n => n.Id == child.Id).ParentId);
    }

    [Fact]
    public async Task Notes_can_nest_several_levels_deep()
    {
        var client = await AuthenticatedClientAsync();
        var top = await CreateNamedNoteAsync(client, "Top");
        var middle = await CreateNamedNoteAsync(client, "Middle", top.Id);

        var bottom = await CreateNamedNoteAsync(client, "Bottom", middle.Id);

        var fetched = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{bottom.Id}");
        Assert.Equal(middle.Id, fetched!.ParentId);
    }

    [Fact]
    public async Task Sibling_notes_may_share_the_same_name()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await CreateNamedNoteAsync(client, "Parent");

        await CreateNamedNoteAsync(client, "Same name", parent.Id);
        var second = await CreateNamedNoteAsync(client, "same NAME", parent.Id);

        Assert.Equal("same NAME", second.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_rejects_an_empty_name(string name)
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = name, Content = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_rejects_a_name_longer_than_200_characters()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = new string('x', 201), Content = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_trims_the_name()
    {
        var client = await AuthenticatedClientAsync();

        var created = await CreateNamedNoteAsync(client, "  Trim me  ");

        Assert.Equal("Trim me", created.Name);
    }

    [Fact]
    public async Task Create_under_a_missing_parent_is_a_bad_request()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Orphan", Content = string.Empty, ParentId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_under_another_users_note_is_a_bad_request()
    {
        var owner = await AuthenticatedClientAsync();
        var ownersNote = await CreateNamedNoteAsync(owner, "Private parent");
        using var intruder = await AuthenticatedClientAsync(_factory.CreateClient());

        var response = await intruder.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Sneaky child", Content = string.Empty, ParentId = ownersNote.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_under_a_trashed_parent_is_a_bad_request()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await CreateNamedNoteAsync(client, "Soon trashed");
        await client.DeleteAsync($"/api/notes/{parent.Id}");

        var response = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Late child", Content = string.Empty, ParentId = parent.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rename_changes_the_name_and_leaves_the_content_alone()
    {
        var client = await AuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Old name", Content = "body text" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var response = await client.PutAsJsonAsync($"/api/notes/{created!.Id}/name", new RenameNoteRequest { Name = "New name" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{created.Id}");
        Assert.Equal("New name", fetched!.Name);
        Assert.Equal("body text", fetched.Content);
    }

    [Fact]
    public async Task Rename_rejects_an_empty_name()
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateNamedNoteAsync(client, "Keep me");

        var response = await client.PutAsJsonAsync($"/api/notes/{created.Id}/name", new RenameNoteRequest { Name = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fetched = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{created.Id}");
        Assert.Equal("Keep me", fetched!.Name);
    }

    [Fact]
    public async Task Rename_of_someone_elses_note_returns_not_found()
    {
        var owner = await AuthenticatedClientAsync();
        var created = await CreateNamedNoteAsync(owner, "Mine");
        using var intruder = await AuthenticatedClientAsync(_factory.CreateClient());

        var response = await intruder.PutAsJsonAsync($"/api/notes/{created.Id}/name", new RenameNoteRequest { Name = "Taken over" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rename_of_a_trashed_note_returns_not_found()
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateNamedNoteAsync(client, "In the bin");
        await client.DeleteAsync($"/api/notes/{created.Id}");

        var response = await client.PutAsJsonAsync($"/api/notes/{created.Id}/name", new RenameNoteRequest { Name = "Renamed" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rename_rejects_requests_without_a_token()
    {
        var response = await _factory.CreateClient().PutAsJsonAsync($"/api/notes/{Guid.NewGuid()}/name", new RenameNoteRequest { Name = "x" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Moving a Note (Spec 1, ticket 1/7) ---

    private static Task<HttpResponseMessage> MoveAsync(HttpClient client, Guid id, Guid? parentId) =>
        client.PutAsJsonAsync($"/api/notes/{id}/parent", new MoveNoteRequest { ParentId = parentId });

    [Fact]
    public async Task Move_puts_a_note_under_another_parent_and_keeps_its_children()
    {
        var client = await AuthenticatedClientAsync();
        var newParent = await CreateNamedNoteAsync(client, "new parent");
        var moved = await CreateNamedNoteAsync(client, "moved");
        var child = await CreateNamedNoteAsync(client, "child", moved.Id);

        var response = await MoveAsync(client, moved.Id, newParent.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal(newParent.Id, result!.ParentId);
        var active = await ActiveAsync(client);
        Assert.Equal(newParent.Id, active.Single(n => n.Id == moved.Id).ParentId);
        Assert.Equal(moved.Id, active.Single(n => n.Id == child.Id).ParentId);
    }

    [Fact]
    public async Task Move_to_null_brings_a_note_up_to_the_top_level()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await CreateNamedNoteAsync(client, "parent");
        var child = await CreateNamedNoteAsync(client, "child", parent.Id);

        var response = await MoveAsync(client, child.Id, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null((await response.Content.ReadFromJsonAsync<NoteResponse>())!.ParentId);
    }

    [Fact]
    public async Task Move_under_itself_or_a_descendant_is_a_conflict_and_changes_nothing()
    {
        var client = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(client, "root");
        var child = await CreateNamedNoteAsync(client, "child", root.Id);
        var grandchild = await CreateNamedNoteAsync(client, "grandchild", child.Id);

        Assert.Equal(HttpStatusCode.Conflict, (await MoveAsync(client, root.Id, root.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await MoveAsync(client, root.Id, child.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await MoveAsync(client, root.Id, grandchild.Id)).StatusCode);

        Assert.Null((await ActiveAsync(client)).Single(n => n.Id == root.Id).ParentId);
    }

    [Fact]
    public async Task Move_under_a_missing_trashed_or_other_users_note_is_a_bad_request()
    {
        var client = await AuthenticatedClientAsync();
        var note = await CreateNamedNoteAsync(client, "note");
        var trashed = await CreateNamedNoteAsync(client, "trashed");
        await client.DeleteAsync($"/api/notes/{trashed.Id}");
        using var other = await AuthenticatedClientAsync(_factory.CreateClient());
        var othersNote = await CreateNamedNoteAsync(other, "theirs");

        Assert.Equal(HttpStatusCode.BadRequest, (await MoveAsync(client, note.Id, Guid.NewGuid())).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await MoveAsync(client, note.Id, trashed.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await MoveAsync(client, note.Id, othersNote.Id)).StatusCode);
    }

    [Fact]
    public async Task Move_to_a_parent_with_a_same_named_child_succeeds_with_a_duplicate_name_warning()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await CreateNamedNoteAsync(client, "parent");
        await CreateNamedNoteAsync(client, "Ideas", parent.Id);
        var moved = await CreateNamedNoteAsync(client, "ideas");

        var response = await MoveAsync(client, moved.Id, parent.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadFromJsonAsync<NoteResponse>())!.HasDuplicateSiblingName);
    }

    [Fact]
    public async Task Move_of_someone_elses_or_a_trashed_or_unknown_note_returns_not_found()
    {
        var owner = await AuthenticatedClientAsync();
        var note = await CreateNamedNoteAsync(owner, "mine");
        var trashed = await CreateNamedNoteAsync(owner, "trashed");
        await owner.DeleteAsync($"/api/notes/{trashed.Id}");
        using var intruder = await AuthenticatedClientAsync(_factory.CreateClient());

        Assert.Equal(HttpStatusCode.NotFound, (await MoveAsync(intruder, note.Id, null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await MoveAsync(owner, trashed.Id, null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await MoveAsync(owner, Guid.NewGuid(), null)).StatusCode);
    }

    [Fact]
    public async Task Move_rejects_requests_without_a_token()
    {
        var response = await MoveAsync(_factory.CreateClient(), Guid.NewGuid(), null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Whole-tree Trash (Spec 1, ticket 2/7) ---

    private static Task<HttpResponseMessage> RestoreAsync(HttpClient client, Guid id) =>
        client.PostAsync($"/api/notes/{id}/restore", null);

    private static async Task<List<NoteResponse>> ActiveAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes"))!;

    private static async Task<List<NoteResponse>> TrashAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<List<NoteResponse>>("/api/notes/trash"))!;

    [Fact]
    public async Task Deleting_a_note_sends_all_its_descendants_to_trash_too()
    {
        var client = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(client, "root");
        var child = await CreateNamedNoteAsync(client, "child", root.Id);
        var grandchild = await CreateNamedNoteAsync(client, "grandchild", child.Id);

        var response = await client.DeleteAsync($"/api/notes/{root.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var active = await ActiveAsync(client);
        Assert.DoesNotContain(active, n => n.Id == root.Id || n.Id == child.Id || n.Id == grandchild.Id);
    }

    [Fact]
    public async Task Trash_lists_only_the_root_of_each_deletion()
    {
        var client = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(client, "root");
        var child = await CreateNamedNoteAsync(client, "child", root.Id);
        await client.DeleteAsync($"/api/notes/{root.Id}");

        var trash = await TrashAsync(client);

        Assert.Contains(trash, n => n.Id == root.Id);
        Assert.DoesNotContain(trash, n => n.Id == child.Id);
    }

    [Fact]
    public async Task Restoring_the_root_brings_the_whole_tree_back()
    {
        var client = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(client, "root");
        var child = await CreateNamedNoteAsync(client, "child", root.Id);
        var grandchild = await CreateNamedNoteAsync(client, "grandchild", child.Id);
        await client.DeleteAsync($"/api/notes/{root.Id}");

        var response = await RestoreAsync(client, root.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var active = await ActiveAsync(client);
        Assert.Contains(active, n => n.Id == root.Id && n.DeletedAt == null);
        Assert.Contains(active, n => n.Id == child.Id && n.ParentId == root.Id);
        Assert.Contains(active, n => n.Id == grandchild.Id && n.ParentId == child.Id);
        Assert.DoesNotContain(await TrashAsync(client), n => n.Id == root.Id);
    }

    [Fact]
    public async Task Restoring_the_root_does_not_restore_a_child_deleted_on_its_own_earlier()
    {
        var client = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(client, "root");
        var earlier = await CreateNamedNoteAsync(client, "deleted earlier", root.Id);
        var sibling = await CreateNamedNoteAsync(client, "deleted with root", root.Id);
        await client.DeleteAsync($"/api/notes/{earlier.Id}");
        await client.DeleteAsync($"/api/notes/{root.Id}");

        await RestoreAsync(client, root.Id);

        var active = await ActiveAsync(client);
        Assert.Contains(active, n => n.Id == sibling.Id);
        Assert.DoesNotContain(active, n => n.Id == earlier.Id);
        Assert.Contains(await TrashAsync(client), n => n.Id == earlier.Id);
    }

    [Fact]
    public async Task Restoring_a_child_while_its_parent_is_still_in_trash_is_a_conflict()
    {
        var client = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(client, "root");
        var child = await CreateNamedNoteAsync(client, "child", root.Id);
        await client.DeleteAsync($"/api/notes/{child.Id}");
        await client.DeleteAsync($"/api/notes/{root.Id}");

        var response = await RestoreAsync(client, child.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain(await ActiveAsync(client), n => n.Id == child.Id);
    }

    [Fact]
    public async Task Restoring_a_note_whose_name_matches_another_succeeds_with_a_duplicate_name_warning()
    {
        var client = await AuthenticatedClientAsync();
        var original = await CreateNamedNoteAsync(client, "Shopping");
        await client.DeleteAsync($"/api/notes/{original.Id}");
        await CreateNamedNoteAsync(client, "shopping");

        var response = await RestoreAsync(client, original.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var restored = await response.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.True(restored!.HasDuplicateSiblingName);
        Assert.Contains(await ActiveAsync(client), n => n.Id == original.Id);
    }

    [Fact]
    public async Task Create_and_rename_warn_about_a_duplicate_sibling_name_without_failing()
    {
        var client = await AuthenticatedClientAsync();
        var parent = await CreateNamedNoteAsync(client, "parent");
        await CreateNamedNoteAsync(client, "Ideas", parent.Id);

        var duplicate = await CreateNamedNoteAsync(client, "IDEAS", parent.Id);
        var other = await CreateNamedNoteAsync(client, "Other", parent.Id);
        var renamed = await (await client.PutAsJsonAsync($"/api/notes/{other.Id}/name", new RenameNoteRequest { Name = "ideas" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        var elsewhere = await CreateNamedNoteAsync(client, "Ideas");

        Assert.True(duplicate.HasDuplicateSiblingName);
        Assert.True(renamed!.HasDuplicateSiblingName);
        Assert.False(elsewhere.HasDuplicateSiblingName);
    }

    [Fact]
    public async Task Restoring_a_note_unfiled_from_its_topic_also_unfiles_it_in_search()
    {
        var client = await AuthenticatedClientAsync();
        var topic = await (await client.PostAsJsonAsync("/api/topics", new CreateTopicRequest { Name = "Reused" }))
            .Content.ReadFromJsonAsync<TopicResponse>();
        var original = await CreateNamedNoteAsync(client, "original");
        await client.PutAsJsonAsync($"/api/notes/{original.Id}/topic", new AssignNoteTopicRequest { TopicId = topic!.Id });
        await client.DeleteAsync($"/api/notes/{original.Id}");
        await client.PutAsJsonAsync($"/api/notes/by-topic/{topic.Id}", new UpdateNoteRequest { Content = "new occupant" });

        await RestoreAsync(client, original.Id);

        var inTopic = await client.GetFromJsonAsync<List<SearchResultResponse>>($"/api/search?topicId={topic.Id}");
        Assert.DoesNotContain(inTopic!, r => r.Id == original.Id);
    }

    [Fact]
    public async Task Restoring_does_not_change_the_notes_updated_at()
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateNamedNoteAsync(client, "keeps its place");
        // Read back from the database: the create response carries a finer timestamp than Postgres stores.
        var before = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{created.Id}");
        await client.DeleteAsync($"/api/notes/{created.Id}");

        var restored = await (await RestoreAsync(client, created.Id)).Content.ReadFromJsonAsync<NoteResponse>();

        Assert.Equal(before!.UpdatedAt, restored!.UpdatedAt);
    }

    [Fact]
    public async Task Restoring_a_note_with_a_unique_name_has_no_duplicate_name_warning()
    {
        var client = await AuthenticatedClientAsync();
        var note = await CreateNamedNoteAsync(client, "Only one");
        await client.DeleteAsync($"/api/notes/{note.Id}");

        var restored = await (await RestoreAsync(client, note.Id)).Content.ReadFromJsonAsync<NoteResponse>();

        Assert.False(restored!.HasDuplicateSiblingName);
    }

    [Fact]
    public async Task Restoring_an_active_or_unknown_note_returns_not_found()
    {
        var client = await AuthenticatedClientAsync();
        var active = await CreateNamedNoteAsync(client, "active");

        Assert.Equal(HttpStatusCode.NotFound, (await RestoreAsync(client, active.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await RestoreAsync(client, Guid.NewGuid())).StatusCode);
    }

    [Fact]
    public async Task Another_user_cannot_delete_or_restore_my_tree()
    {
        var owner = await AuthenticatedClientAsync();
        var root = await CreateNamedNoteAsync(owner, "root");
        var child = await CreateNamedNoteAsync(owner, "child", root.Id);
        using var other = await AuthenticatedClientAsync(_factory.CreateClient());

        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"/api/notes/{root.Id}")).StatusCode);
        await owner.DeleteAsync($"/api/notes/{root.Id}");
        Assert.Equal(HttpStatusCode.NotFound, (await RestoreAsync(other, root.Id)).StatusCode);
        Assert.DoesNotContain(await TrashAsync(other), n => n.Id == root.Id || n.Id == child.Id);
    }
}
