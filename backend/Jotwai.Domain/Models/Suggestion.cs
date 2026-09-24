namespace Jotwai.Domain.Models;

/// <summary>
/// A ranked set of Category candidates for a Note (GLOSSARY.md "Suggestion"). A Note has at
/// most one active (<see cref="ResolvedAt"/> null) Suggestion at a time.
/// </summary>
public sealed class Suggestion
{
    public Guid Id { get; init; }
    public required Guid NoteId { get; init; }
    public required Guid UserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
