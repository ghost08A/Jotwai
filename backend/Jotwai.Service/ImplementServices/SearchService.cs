using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class SearchService(SearchDbContext db) : ISearchService
{
    public async Task<IReadOnlyList<SearchResultResponse>> SearchAsync(Guid userId, string? query, Guid? topicId, Guid? parentId, IReadOnlyList<Guid>? categoryIds, CancellationToken cancellationToken = default)
    {
        var documents = db.SearchDocuments.AsNoTracking()
            .Where(d => d.UserId == userId && d.DeletedAt == null);

        if (topicId is { } filterTopicId)
        {
            var topicIds = await DescendantTopicIdsAsync(userId, filterTopicId, cancellationToken);
            documents = documents.Where(d => d.TopicId != null && topicIds.Contains(d.TopicId.Value));
        }

        if (parentId is { } filterParentId)
        {
            var noteIds = await SubtreeNoteIdsAsync(userId, filterParentId, cancellationToken);
            documents = documents.Where(d => noteIds.Contains(d.NoteId));
        }

        if (categoryIds is { Count: > 0 } requiredCategoryIds)
        {
            // A Note must carry every requested Category (AND) - resolved to Note IDs up
            // front for the same reason the category-name match below is: a per-Category
            // "does this Note have it" check isn't expressible as a single EF join predicate.
            var matchingNoteIds = await db.SearchDocumentCategories.AsNoTracking()
                .Where(dc => requiredCategoryIds.Contains(dc.CategoryId))
                .GroupBy(dc => dc.NoteId)
                .Where(g => g.Select(dc => dc.CategoryId).Distinct().Count() == requiredCategoryIds.Count)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            documents = documents.Where(d => matchingNoteIds.Contains(d.NoteId));
        }

        var trimmed = query?.Trim();
        if (!string.IsNullOrEmpty(trimmed))
        {
            var pattern = $"%{EscapeLikePattern(trimmed)}%";

            // Category name matches are resolved to Note IDs up front - a single-column
            // pattern match can't be expressed as an EF join predicate the way ILIKE on the
            // document's own columns can below.
            var matchingNoteIds = await db.SearchDocumentCategories.AsNoTracking()
                .Join(db.SearchCategories.AsNoTracking(), dc => dc.CategoryId, c => c.CategoryId, (dc, c) => new { dc.NoteId, c.Name })
                .Where(x => EF.Functions.ILike(x.Name, pattern, "\\"))
                .Select(x => x.NoteId)
                .Distinct()
                .ToListAsync(cancellationToken);

            documents = documents.Where(d =>
                EF.Functions.ILike(d.Name, pattern, "\\") ||
                EF.Functions.ILike(d.Content, pattern, "\\") ||
                (d.TopicName != null && EF.Functions.ILike(d.TopicName, pattern, "\\")) ||
                matchingNoteIds.Contains(d.NoteId));
        }

        var results = await documents
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);

        return await ToResponsesAsync(results, cancellationToken);
    }

    // The Note itself plus all its descendants, walked over the Search read model's own
    // (NoteId, ParentId) pairs - no join into the notes schema (ADR-0003). Trashed Notes are
    // left out of the walk, so a trashed Note takes its whole subtree out of the filter.
    private async Task<HashSet<Guid>> SubtreeNoteIdsAsync(Guid userId, Guid rootNoteId, CancellationToken cancellationToken)
    {
        var links = await db.SearchDocuments.AsNoTracking()
            .Where(d => d.UserId == userId && d.DeletedAt == null && d.ParentId != null)
            .Select(d => new { d.NoteId, ParentId = d.ParentId!.Value })
            .ToListAsync(cancellationToken);
        var childrenOf = links.ToLookup(l => l.ParentId, l => l.NoteId);

        var ids = new List<Guid> { rootNoteId };
        for (var i = 0; i < ids.Count; i++)
            ids.AddRange(childrenOf[ids[i]]);
        return ids.ToHashSet();
    }

    private async Task<HashSet<Guid>> DescendantTopicIdsAsync(Guid userId, Guid rootTopicId, CancellationToken cancellationToken)
    {
        var topics = await db.SearchTopics.AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new { t.TopicId, t.ParentId })
            .ToListAsync(cancellationToken);

        var childrenByParent = topics
            .Where(t => t.ParentId is not null)
            .GroupBy(t => t.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(t => t.TopicId).ToList());

        var descendantIds = new HashSet<Guid> { rootTopicId };
        var pending = new Stack<Guid>();
        pending.Push(rootTopicId);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!childrenByParent.TryGetValue(current, out var children))
                continue;

            foreach (var childId in children)
            {
                if (descendantIds.Add(childId))
                    pending.Push(childId);
            }
        }

        return descendantIds;
    }

    private async Task<IReadOnlyList<SearchResultResponse>> ToResponsesAsync(List<SearchDocument> documents, CancellationToken cancellationToken)
    {
        var noteIds = documents.Select(d => d.NoteId).ToList();
        var categoriesByNoteId = await db.SearchDocumentCategories.AsNoTracking()
            .Where(dc => noteIds.Contains(dc.NoteId))
            .GroupBy(dc => dc.NoteId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(dc => dc.CategoryId).ToList(), cancellationToken);

        return documents.Select(d => new SearchResultResponse
        {
            Id = d.NoteId,
            Name = d.Name,
            ParentId = d.ParentId,
            Content = d.Content,
            TopicId = d.TopicId,
            CategoryIds = categoriesByNoteId.GetValueOrDefault(d.NoteId, []),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
        }).ToList();
    }

    // Postgres LIKE/ILIKE treats % and _ as wildcards - escape any the user typed literally
    // so e.g. searching "50%" doesn't match "50" followed by anything.
    private static string EscapeLikePattern(string term) =>
        term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
