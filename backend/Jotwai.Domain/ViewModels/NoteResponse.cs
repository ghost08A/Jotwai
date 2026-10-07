namespace Jotwai.Domain.ViewModels;

public sealed class NoteResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Content { get; init; }
    public Guid? ParentId { get; init; }
    public Guid? TopicId { get; init; }
    public required IReadOnlyList<Guid> CategoryIds { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }

    /// <summary>True when another active Note under the same parent has this name (ignoring case).
    /// A warning only - set by create, rename and restore, false everywhere else.</summary>
    public bool HasDuplicateSiblingName { get; init; }
}
