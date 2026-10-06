using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface ISuggestionsService
{
    /// <summary>The Note's active (unresolved) Suggestion, or null if it never got one, the
    /// AI wasn't confident enough (ticket 009), or it was already resolved.</summary>
    Task<SuggestionResponse?> GetActiveAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Accepts a subset of candidates as one atomic transaction (ticket 010): creates
    /// any new Categories needed and attaches every accepted Category to the Note, all or
    /// nothing. Resolves the Suggestion either way - the rest of the candidates are dropped,
    /// not left pending (CONTEXT.md "Suggestion").</summary>
    Task<AcceptSuggestionResult> AcceptAsync(Guid userId, Guid noteId, AcceptSuggestionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Resolves the Suggestion without accepting anything. Returns false if there's
    /// no active Suggestion to ignore.</summary>
    Task<bool> IgnoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);
}
