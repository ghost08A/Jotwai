namespace Jotwai.Domain.Models;

/// <summary>
/// The Search Module's own read model (ADR-0003) of the Organization Module's Categories,
/// kept in sync by subscribing to CategoryCreated/CategoryRenamed/CategoryDeleted. Backs
/// matching a search term against a Note's Category names (CONTEXT.md "Search") without
/// querying the `organization` schema directly.
/// </summary>
public sealed class SearchCategory
{
    public required Guid CategoryId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; set; }
}
