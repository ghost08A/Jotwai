namespace Jotwai.Domain.Models;

public sealed class Topic
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public Guid? ParentId { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Denormalized count of active Notes currently assigned to this Topic - the
    /// Organization Module's own read model (ADR-0003), kept in sync by subscribing to
    /// the Notes Module's <see cref="Events.NoteTopicAssigned"/>. Backs the "a Topic with
    /// Notes can't be deleted" guard without querying the `notes` schema directly.
    /// </summary>
    public int NoteCount { get; set; }
}
