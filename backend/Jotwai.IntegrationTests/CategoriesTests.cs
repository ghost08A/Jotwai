using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;

namespace Jotwai.IntegrationTests;

public sealed class CategoriesTests : IClassFixture<JotwaiApiFactory>
{
    private readonly JotwaiApiFactory _factory;
    private readonly HttpClient _client;

    public CategoriesTests(JotwaiApiFactory factory)
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
    public async Task Create_category_succeeds()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.Equal("Ideas", category!.Name);
    }

    [Fact]
    public async Task Create_with_duplicate_name_case_insensitive_is_rejected()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" });

        var response = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "ideas" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Different_users_can_each_have_a_category_with_the_same_name()
    {
        var userA = await AuthenticatedClientAsync();
        var createA = await userA.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" });

        using var userB = await AuthenticatedClientAsync(_factory.CreateClient());
        var createB = await userB.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" });

        Assert.Equal(HttpStatusCode.Created, createA.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createB.StatusCode);
    }

    [Fact]
    public async Task Rename_to_an_existing_name_is_rejected()
    {
        var client = await AuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" });
        var other = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Archive" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();

        var response = await client.PutAsJsonAsync($"/api/categories/{other!.Id}", new RenameCategoryRequest { Name = "Ideas" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_category_succeeds_even_when_notes_are_attached()
    {
        var client = await AuthenticatedClientAsync();
        var category = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PostAsync($"/api/notes/{note!.Id}/categories/{category!.Id}", null);

        var response = await client.DeleteAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Attaching_multiple_categories_to_a_note_is_reflected_in_the_note()
    {
        var client = await AuthenticatedClientAsync();
        var work = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Work" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var urgent = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Urgent" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        await client.PostAsync($"/api/notes/{note!.Id}/categories/{work!.Id}", null);
        var afterSecond = await client.PostAsync($"/api/notes/{note.Id}/categories/{urgent!.Id}", null);
        var updated = await afterSecond.Content.ReadFromJsonAsync<NoteResponse>();

        Assert.Equal(HttpStatusCode.OK, afterSecond.StatusCode);
        Assert.Contains(work.Id, updated!.CategoryIds);
        Assert.Contains(urgent.Id, updated.CategoryIds);
        Assert.Equal(2, updated.CategoryIds.Count);
    }

    [Fact]
    public async Task Attaching_the_same_category_twice_is_idempotent()
    {
        var client = await AuthenticatedClientAsync();
        var category = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        await client.PostAsync($"/api/notes/{note!.Id}/categories/{category!.Id}", null);
        var second = await client.PostAsync($"/api/notes/{note.Id}/categories/{category.Id}", null);
        var updated = await second.Content.ReadFromJsonAsync<NoteResponse>();

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Single(updated!.CategoryIds);
    }

    [Fact]
    public async Task Attaching_a_nonexistent_category_returns_bad_request()
    {
        var client = await AuthenticatedClientAsync();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();

        var response = await client.PostAsync($"/api/notes/{note!.Id}/categories/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Detaching_a_category_removes_it_from_the_note()
    {
        var client = await AuthenticatedClientAsync();
        var category = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PostAsync($"/api/notes/{note!.Id}/categories/{category!.Id}", null);

        var response = await client.DeleteAsync($"/api/notes/{note.Id}/categories/{category.Id}");
        var updated = await response.Content.ReadFromJsonAsync<NoteResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(updated!.CategoryIds);
    }

    [Fact]
    public async Task Deleting_a_category_detaches_it_from_notes_that_had_it()
    {
        var client = await AuthenticatedClientAsync();
        var category = await (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Ideas" }))
            .Content.ReadFromJsonAsync<CategoryResponse>();
        var note = await (await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest { Name = "Test note", Content = "note" }))
            .Content.ReadFromJsonAsync<NoteResponse>();
        await client.PostAsync($"/api/notes/{note!.Id}/categories/{category!.Id}", null);

        await client.DeleteAsync($"/api/categories/{category.Id}");

        var refreshed = await client.GetFromJsonAsync<NoteResponse>($"/api/notes/{note.Id}");
        Assert.Empty(refreshed!.CategoryIds);
    }

    [Fact]
    public async Task Categories_endpoints_reject_requests_without_a_token()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
