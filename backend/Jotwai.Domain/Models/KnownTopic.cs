namespace Jotwai.Domain.Models;

/// <summary>
/// The Notes Module's own read model (ADR-0003) of Topic IDs that currently exist,
/// kept in sync by subscribing to the Organization Module's TopicCreated/TopicDeleted.
/// Lets Notes validate a Note's Topic assignment without querying the `organization`
/// schema directly.
/// </summary>
public sealed class KnownTopic
{
    public required Guid TopicId { get; init; }
    public required Guid UserId { get; init; }
}
