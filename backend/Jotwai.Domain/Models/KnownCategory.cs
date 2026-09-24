namespace Jotwai.Domain.Models;

/// <summary>
/// The Notes Module's own read model (ADR-0003) of Category IDs that currently exist,
/// kept in sync by subscribing to the Organization Module's CategoryCreated/CategoryDeleted.
/// Lets Notes validate attaching a Category to a Note without querying the `organization`
/// schema directly.
/// </summary>
public sealed class KnownCategory
{
    public required Guid CategoryId { get; init; }
    public required Guid UserId { get; init; }
}
