using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface INotesService
{
    /// <summary>Creates a Note with a required name, optionally under a parent Note. Fails with
    /// <see cref="NoteWriteFailureReason.InvalidName"/> if the name is empty or longer than 200
    /// characters once trimmed, or <see cref="NoteWriteFailureReason.ParentNotFound"/> if the
    /// parent doesn't exist for this user or is in the Trash.</summary>
    Task<NoteWriteResult> CreateAsync(Guid userId, CreateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Changes only the Note's name - never its content, so it can't clash with
    /// autosave. Same name rules as <see cref="CreateAsync"/>; not found (or in the Trash) is
    /// <see cref="NoteWriteFailureReason.NotFound"/>.</summary>
    Task<NoteWriteResult> RenameAsync(Guid userId, Guid noteId, RenameNoteRequest request, CancellationToken cancellationToken = default);

    Task<NoteResponse?> GetByIdAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NoteResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NoteResponse>> ListTrashAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Overwrites the Note's content - Notes have no version history (CONTEXT.md).</summary>
    Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>The Topic's one active Note (a Topic has at most one - see
    /// <c>NoteConfiguration</c>'s unique filtered index), or null if the Topic has no
    /// content yet.</summary>
    Task<NoteResponse?> GetByTopicAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);

    /// <summary>Creates the Topic's Note on first call, overwrites its content (like
    /// <see cref="UpdateAsync"/>) on every call after that - the continuous-document backing
    /// for the Topic Detail page. Fails with <see cref="UpsertTopicNoteFailureReason.TopicNotFound"/>
    /// if the Topic doesn't exist.</summary>
    Task<UpsertTopicNoteResult> UpsertByTopicAsync(Guid userId, Guid topicId, UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes the Note into the Trash. Returns false if not found/owned or already deleted.</summary>
    Task<bool> DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Restores a Note out of the Trash. Returns false if not found/owned or not deleted.</summary>
    Task<bool> RestoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Moves the Note into a Topic, or clears it when <paramref name="topicId"/> is null.
    /// The Topic must already exist (ticket 036) - this never creates one.</summary>
    Task<AssignNoteTopicResult> AssignTopicAsync(Guid userId, Guid noteId, Guid? topicId, CancellationToken cancellationToken = default);

    /// <summary>Attaches a Category to the Note (idempotent - already attached is still a success).
    /// The Category must already exist.</summary>
    Task<NoteCategoryAttachResult> AttachCategoryAsync(Guid userId, Guid noteId, Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>Detaches a Category from the Note (idempotent - not attached is still a success).
    /// Returns null if the Note isn't found/owned.</summary>
    Task<NoteResponse?> DetachCategoryAsync(Guid userId, Guid noteId, Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>Explicit user action to (re-)run AI Categorization on demand (CONTEXT.md
    /// "AI Categorization") - unlike the automatic path, this ignores
    /// <c>AiCategorizationTriggeredAt</c> and can be called any number of times; it doesn't
    /// count as the one automatic trigger and doesn't affect it. Returns false if the Note
    /// isn't found/owned or is in the Trash.</summary>
    Task<bool> TriggerCategorizationAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);
}
