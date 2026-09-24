namespace Jotwai.Domain.Models;

/// <summary>
/// The AI Categorization Module's own read model (ADR-0003) of the Organization Module's
/// Categories, kept in sync via CategoryCreated/CategoryRenamed/CategoryDeleted. Needed for
/// two things without querying the `organization` schema directly: giving the AI provider
/// the user's existing Category names as context, and deciding whether a suggested name
/// matches an existing Category or should be offered as a new one.
/// </summary>
public sealed class CategoryProjection
{
    public required Guid CategoryId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; set; }
}
