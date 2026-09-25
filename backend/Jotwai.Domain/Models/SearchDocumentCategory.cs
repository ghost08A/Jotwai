namespace Jotwai.Domain.Models;

/// <summary>
/// Join between a <see cref="SearchDocument"/> and a Category it's tagged with - the Search
/// Module's own copy (ADR-0003) of the Notes Module's NoteCategory, kept in sync by
/// subscribing to <see cref="Events.NoteCategoryAttached"/>/<see cref="Events.NoteCategoryDetached"/>.
/// </summary>
public sealed class SearchDocumentCategory
{
    public required Guid NoteId { get; init; }
    public required Guid CategoryId { get; init; }
}
