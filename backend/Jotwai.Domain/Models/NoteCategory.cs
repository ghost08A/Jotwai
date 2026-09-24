namespace Jotwai.Domain.Models;

/// <summary>
/// The many-to-many join between a Note and the Categories attached to it - owned by the
/// Notes Module (schema `notes`), the same way Note.TopicId is: no provenance tracked
/// (ticket 032), and CategoryId is a plain value with no cross-schema FK (ADR-0003).
/// </summary>
public sealed class NoteCategory
{
    public required Guid NoteId { get; init; }
    public required Guid CategoryId { get; init; }
}
