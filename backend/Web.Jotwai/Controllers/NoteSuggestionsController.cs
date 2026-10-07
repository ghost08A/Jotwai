using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.ValidateModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Jotwai.Controllers;

// Note Suggestion (Placement) - kept apart from SuggestionsController, whose
// `api/notes/{noteId}/suggestion` is the Category Suggestion.
[Authorize]
[ApiController]
[Route("api/notes/{noteId:guid}/note-suggestion")]
public sealed class NoteSuggestionsController(INoteSuggestionsService noteSuggestionsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive(Guid noteId, CancellationToken cancellationToken)
    {
        var suggestion = await noteSuggestionsService.GetActiveAsync(CurrentUserId(), noteId, cancellationToken);
        return suggestion is null ? NotFound() : Ok(suggestion);
    }

    // Absolute route (not nested under {noteId}) - powers the indicator on the Notes page with
    // one call instead of one per Note.
    [HttpGet("/api/notes/note-suggestions")]
    public async Task<IActionResult> ListActive(CancellationToken cancellationToken)
    {
        var noteIds = await noteSuggestionsService.ListActiveNoteIdsAsync(CurrentUserId(), cancellationToken);
        return Ok(noteIds);
    }

    // Accepting a Placement moves the Note at once - the user already confirmed by clicking.
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(Guid noteId, CancellationToken cancellationToken)
    {
        var result = await noteSuggestionsService.AcceptAsync(CurrentUserId(), noteId, cancellationToken);

        return result.Succeeded
            ? NoContent()
            : result.FailureReason switch
            {
                AcceptNoteSuggestionFailureReason.SuggestionNotFound => NotFound(),
                AcceptNoteSuggestionFailureReason.TargetNotAvailable => Conflict(new { error = "The suggested Note can no longer be a parent of this Note." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
    }

    [HttpPost("ignore")]
    public async Task<IActionResult> Ignore(Guid noteId, CancellationToken cancellationToken)
    {
        var ignored = await noteSuggestionsService.IgnoreAsync(CurrentUserId(), noteId, cancellationToken);
        return ignored ? NoContent() : NotFound();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
