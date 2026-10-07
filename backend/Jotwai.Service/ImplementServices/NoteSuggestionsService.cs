using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class NoteSuggestionsService(AiCategorizationDbContext db, INotesService notesService) : INoteSuggestionsService
{
    public async Task<NoteSuggestionResponse?> GetActiveAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.NoteSuggestions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.NoteId == noteId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return null;

        var targetName = await db.NoteProjections.AsNoTracking()
            .Where(n => n.NoteId == suggestion.TargetNoteId)
            .Select(n => n.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? "(unknown)";

        return ToResponse(suggestion, targetName);
    }

    public async Task<IReadOnlyList<Guid>> ListActiveNoteIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.NoteSuggestions.AsNoTracking()
            .Where(s => s.UserId == userId && s.ResolvedAt == null)
            .Select(s => s.NoteId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IgnoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.NoteSuggestions
            .SingleOrDefaultAsync(s => s.NoteId == noteId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return false;

        suggestion.ResolvedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    // The move touches only the `notes` schema (NotesService.MoveAsync commits its own change), so
    // resolving the suggestion afterward is a separate write: nothing else needs to stay atomic
    // with it, and the worst case on a crash in between is that the suggestion lingers.
    public async Task<AcceptNoteSuggestionResult> AcceptAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.NoteSuggestions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.NoteId == noteId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return AcceptNoteSuggestionResult.Failure(AcceptNoteSuggestionFailureReason.SuggestionNotFound);

        var move = await notesService.MoveAsync(userId, noteId, new MoveNoteRequest { ParentId = suggestion.TargetNoteId }, cancellationToken);
        if (!move.Succeeded)
        {
            return move.FailureReason == NoteWriteFailureReason.NotFound
                ? AcceptNoteSuggestionResult.Failure(AcceptNoteSuggestionFailureReason.SuggestionNotFound)
                : AcceptNoteSuggestionResult.Failure(AcceptNoteSuggestionFailureReason.TargetNotAvailable);
        }

        await db.NoteSuggestions
            .Where(s => s.Id == suggestion.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ResolvedAt, DateTimeOffset.UtcNow), cancellationToken);

        return AcceptNoteSuggestionResult.Success();
    }

    private static NoteSuggestionResponse ToResponse(NoteSuggestion suggestion, string targetName) => new()
    {
        Id = suggestion.Id,
        NoteId = suggestion.NoteId,
        Kind = suggestion.Kind,
        TargetNoteId = suggestion.TargetNoteId,
        TargetNoteName = targetName,
        Confidence = suggestion.Confidence,
        CreatedAt = suggestion.CreatedAt,
    };
}
