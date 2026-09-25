using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface ISearchService
{
    /// <summary>Case-insensitive substring search (ADR-0006) over the current user's Notes,
    /// including Note content, code block content, and the names of the Note's Topic and
    /// Categories. A null/blank <paramref name="query"/> matches every Note. When
    /// <paramref name="topicId"/> is given, only Notes filed under that Topic or one of its
    /// descendants are returned. Results are ordered most-recently-updated first.</summary>
    Task<IReadOnlyList<SearchResultResponse>> SearchAsync(Guid userId, string? query, Guid? topicId, CancellationToken cancellationToken = default);
}
