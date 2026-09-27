namespace Jotwai.Domain.Models;

/// <summary>
/// Nerd Assistant's own read model (ADR-0003) of the Organization Module's Topic hierarchy -
/// needed to give the AI provider every existing Topic name to compare a newly created one
/// against (Topic Suggestion, ticket 040), without querying the `organization` schema
/// directly. Kept in sync via TopicCreated/TopicRenamed/TopicDeleted/TopicParentChanged.
/// </summary>
public sealed class TopicProjection
{
    public required Guid TopicId { get; init; }
    public required Guid UserId { get; init; }
    public Guid? ParentId { get; set; }
    public required string Name { get; set; }
}
