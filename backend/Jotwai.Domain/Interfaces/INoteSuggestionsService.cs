using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface INoteSuggestionsService
{
    /// <summary>The Note's one unresolved Note Suggestion, or null if it has none (or isn't the user's).</summary>
    Task<NoteSuggestionResponse?> GetActiveAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Every Note (by id) that currently has an unresolved Note Suggestion - powers the
    /// indicator on the Notes page with one call instead of one per Note.</summary>
    Task<IReadOnlyList<Guid>> ListActiveNoteIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Resolves the suggestion without changing anything. Returns false if there is no
    /// unresolved suggestion for this user's Note.</summary>
    Task<bool> IgnoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Accepts a Placement: moves the Note under the suggested Note at once (no second
    /// confirmation) and resolves the suggestion. Fails with
    /// <see cref="AcceptNoteSuggestionFailureReason.TargetNotAvailable"/> if the target can no longer
    /// be a parent (trashed, or now a descendant of the Note) - the suggestion stays unresolved.</summary>
    Task<AcceptNoteSuggestionResult> AcceptAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);
}
