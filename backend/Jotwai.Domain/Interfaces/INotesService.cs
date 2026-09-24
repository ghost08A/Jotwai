using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface INotesService
{
    Task<NoteResponse> CreateAsync(Guid userId, CreateNoteRequest request, CancellationToken cancellationToken = default);

    Task<NoteResponse?> GetByIdAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NoteResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NoteResponse>> ListTrashAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Overwrites the Note's content - Notes have no version history (GLOSSARY.md).</summary>
    Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes the Note into the Trash. Returns false if not found/owned or already deleted.</summary>
    Task<bool> DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Restores a Note out of the Trash. Returns false if not found/owned or not deleted.</summary>
    Task<bool> RestoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    /// <summary>Moves the Note into a Topic, or clears it when <paramref name="topicId"/> is null.
    /// The Topic must already exist (ticket 036) - this never creates one.</summary>
    Task<AssignNoteTopicResult> AssignTopicAsync(Guid userId, Guid noteId, Guid? topicId, CancellationToken cancellationToken = default);
}
