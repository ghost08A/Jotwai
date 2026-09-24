namespace Jotwai.Domain.RequestModels;

public sealed class AcceptSuggestionRequest
{
    /// <summary>IDs of the SuggestionCandidate rows to accept - a subset is fine (GLOSSARY.md
    /// "accept partially"). All-or-nothing as a single transaction (ticket 010).</summary>
    public IReadOnlyList<Guid> CandidateIds { get; set; } = [];
}
