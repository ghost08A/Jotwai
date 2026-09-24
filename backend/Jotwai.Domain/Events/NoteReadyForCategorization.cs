using Jotwai.Domain.Interfaces;

namespace Jotwai.Domain.Events;

/// <summary>
/// Published by the Notes Module the first time a Note is saved with real (non-empty)
/// content - never again after that for the same Note (User Ownership, ticket 025). The
/// AI Categorization Module subscribes to this and writes its outbox row (ADR-0004).
/// </summary>
public sealed record NoteReadyForCategorization(Guid NoteId, Guid UserId, string Content, DateTimeOffset TriggeredAt) : IDomainEvent;
