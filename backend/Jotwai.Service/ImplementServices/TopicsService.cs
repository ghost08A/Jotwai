using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class TopicsService(OrganizationDbContext db, IEventPublisher eventPublisher) : ITopicsService
{
    public async Task<TopicWriteResult> CreateAsync(Guid userId, CreateTopicRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ParentId is { } parentId)
        {
            var parentExists = await db.Topics.AnyAsync(t => t.Id == parentId && t.UserId == userId, cancellationToken);
            if (!parentExists)
                return TopicWriteResult.Failure(TopicWriteFailureReason.ParentNotFound);
        }

        var name = request.Name.Trim();
        var normalizedName = Normalize(name);

        var duplicate = await db.Topics.AnyAsync(
            t => t.UserId == userId && t.ParentId == request.ParentId && t.NormalizedName == normalizedName,
            cancellationToken);
        if (duplicate)
            return TopicWriteResult.Failure(TopicWriteFailureReason.DuplicateSiblingName);

        var now = DateTimeOffset.UtcNow;
        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ParentId = request.ParentId,
            Name = name,
            NormalizedName = normalizedName,
            CreatedAt = now,
            UpdatedAt = now,
            NoteCount = 0,
        };

        db.Topics.Add(topic);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new TopicCreated(topic.Id, topic.UserId, topic.ParentId, topic.Name), cancellationToken);

        return TopicWriteResult.Success(ToResponse(topic));
    }

    public async Task<TopicResponse?> GetByIdAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await db.Topics.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == topicId && t.UserId == userId, cancellationToken);

        return topic is null ? null : ToResponse(topic);
    }

    public async Task<IReadOnlyList<TopicResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var topics = await db.Topics.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return topics.Select(ToResponse).ToList();
    }

    public async Task<TopicWriteResult> RenameAsync(Guid userId, Guid topicId, RenameTopicRequest request, CancellationToken cancellationToken = default)
    {
        var topic = await db.Topics.SingleOrDefaultAsync(t => t.Id == topicId && t.UserId == userId, cancellationToken);
        if (topic is null)
            return TopicWriteResult.Failure(TopicWriteFailureReason.NotFound);

        var name = request.Name.Trim();
        var normalizedName = Normalize(name);

        var duplicate = await db.Topics.AnyAsync(
            t => t.Id != topicId && t.UserId == userId && t.ParentId == topic.ParentId && t.NormalizedName == normalizedName,
            cancellationToken);
        if (duplicate)
            return TopicWriteResult.Failure(TopicWriteFailureReason.DuplicateSiblingName);

        topic.Name = name;
        topic.NormalizedName = normalizedName;
        topic.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new TopicRenamed(topic.Id, topic.UserId, topic.Name), cancellationToken);

        return TopicWriteResult.Success(ToResponse(topic));
    }

    public async Task<TopicWriteResult> MoveAsync(Guid userId, Guid topicId, Guid? newParentId, CancellationToken cancellationToken = default)
    {
        var topic = await db.Topics.SingleOrDefaultAsync(t => t.Id == topicId && t.UserId == userId, cancellationToken);
        if (topic is null)
            return TopicWriteResult.Failure(TopicWriteFailureReason.NotFound);

        if (newParentId is { } parentId)
        {
            if (parentId == topicId)
                return TopicWriteResult.Failure(TopicWriteFailureReason.CircularParent);

            var parent = await db.Topics.AsNoTracking()
                .SingleOrDefaultAsync(t => t.Id == parentId && t.UserId == userId, cancellationToken);
            if (parent is null)
                return TopicWriteResult.Failure(TopicWriteFailureReason.ParentNotFound);

            if (await IsDescendantAsync(topicId, parentId, cancellationToken))
                return TopicWriteResult.Failure(TopicWriteFailureReason.CircularParent);
        }

        var duplicate = await db.Topics.AnyAsync(
            t => t.Id != topicId && t.UserId == userId && t.ParentId == newParentId && t.NormalizedName == topic.NormalizedName,
            cancellationToken);
        if (duplicate)
            return TopicWriteResult.Failure(TopicWriteFailureReason.DuplicateSiblingName);

        topic.ParentId = newParentId;
        topic.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new TopicParentChanged(topic.Id, topic.UserId, topic.ParentId), cancellationToken);

        return TopicWriteResult.Success(ToResponse(topic));
    }

    // Walks up from `candidateParentId` toward the root, checking it never passes through
    // `topicId` - true means moving `topicId` under `candidateParentId` would create a cycle.
    private async Task<bool> IsDescendantAsync(Guid topicId, Guid candidateParentId, CancellationToken cancellationToken)
    {
        var currentId = (Guid?)candidateParentId;
        while (currentId is { } id)
        {
            if (id == topicId)
                return true;

            currentId = await db.Topics.AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => t.ParentId)
                .SingleOrDefaultAsync(cancellationToken);
        }

        return false;
    }

    public async Task<TopicDeleteOutcome> DeleteAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await db.Topics.SingleOrDefaultAsync(t => t.Id == topicId && t.UserId == userId, cancellationToken);
        if (topic is null)
            return TopicDeleteOutcome.NotFound;

        if (topic.NoteCount > 0)
            return TopicDeleteOutcome.HasNotes;

        var hasChildTopics = await db.Topics.AnyAsync(t => t.ParentId == topicId, cancellationToken);
        if (hasChildTopics)
            return TopicDeleteOutcome.HasChildTopics;

        db.Topics.Remove(topic);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new TopicDeleted(topic.Id, topic.UserId), cancellationToken);

        return TopicDeleteOutcome.Deleted;
    }

    private static string Normalize(string name) => name.ToLowerInvariant();

    private static TopicResponse ToResponse(Topic topic) => new()
    {
        Id = topic.Id,
        Name = topic.Name,
        ParentId = topic.ParentId,
        CreatedAt = topic.CreatedAt,
        UpdatedAt = topic.UpdatedAt,
    };
}
