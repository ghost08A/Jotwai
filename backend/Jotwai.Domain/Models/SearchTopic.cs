namespace Jotwai.Domain.Models;

/// <summary>
/// The Search Module's own read model (ADR-0003) of the Organization Module's Topic
/// hierarchy, kept in sync by subscribing to TopicCreated/TopicRenamed/TopicDeleted. Lets
/// Search resolve a Topic filter's descendants (ADR-0006 - "filtering by a Topic includes
/// its descendants") without querying the `organization` schema directly.
/// </summary>
public sealed class SearchTopic
{
    public required Guid TopicId { get; init; }
    public required Guid UserId { get; init; }
    public Guid? ParentId { get; set; }
    public required string Name { get; set; }
}
