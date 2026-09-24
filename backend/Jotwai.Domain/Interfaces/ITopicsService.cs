using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface ITopicsService
{
    Task<TopicWriteResult> CreateAsync(Guid userId, CreateTopicRequest request, CancellationToken cancellationToken = default);

    Task<TopicResponse?> GetByIdAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>Flat list of every Topic the user owns - callers assemble the tree from ParentId.</summary>
    Task<IReadOnlyList<TopicResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TopicWriteResult> RenameAsync(Guid userId, Guid topicId, RenameTopicRequest request, CancellationToken cancellationToken = default);

    /// <summary>Fails with HasChildTopics/HasNotes if the Topic isn't empty (GLOSSARY.md).</summary>
    Task<TopicDeleteOutcome> DeleteAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
}
