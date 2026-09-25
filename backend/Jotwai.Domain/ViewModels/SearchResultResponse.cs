namespace Jotwai.Domain.ViewModels;

public sealed class SearchResultResponse
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public Guid? TopicId { get; init; }
    public required IReadOnlyList<Guid> CategoryIds { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
