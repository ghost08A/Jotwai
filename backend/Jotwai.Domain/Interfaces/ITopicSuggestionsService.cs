using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface ITopicSuggestionsService
{
    Task<TopicSuggestionResponse?> GetActiveAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>Every Topic (by id) that currently has an active Topic Suggestion - powers
    /// the badge indicator on the Topic Browser page (ticket 040) without an N+1 fetch.</summary>
    Task<IReadOnlyList<Guid>> ListActiveTopicIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IgnoreAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>Accepts whichever active Topic Suggestion this Topic has - Merge (deletes this
    /// Topic, reassigns its Notes and subtopics to the target) or Placement (moves this Topic
    /// under the target). The client is responsible for its own extra confirm-dialog before
    /// calling this for a Merge (CONTEXT.md) - this method just performs the action.</summary>
    Task<AcceptTopicSuggestionResult> AcceptAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>Explicit user action to (re-)run Topic Suggestion on demand, or automatically
    /// when the user leaves the Topic Detail page after editing (ticket 044) - unlike the
    /// one-time automatic trigger on Topic creation, this can be called any number of times
    /// and always analyzes the Topic's *current* name (from <c>TopicProjections</c>) plus
    /// whatever content is passed in. Returns false if the Topic isn't found/owned.</summary>
    Task<bool> TriggerAsync(Guid userId, Guid topicId, string content, CancellationToken cancellationToken = default);
}
