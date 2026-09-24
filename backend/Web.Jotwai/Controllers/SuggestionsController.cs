using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Jotwai.Controllers;

[Authorize]
[ApiController]
[Route("api/notes/{noteId:guid}/suggestion")]
public sealed class SuggestionsController(ISuggestionsService suggestionsService, INotesService notesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive(Guid noteId, CancellationToken cancellationToken)
    {
        var suggestion = await suggestionsService.GetActiveAsync(CurrentUserId(), noteId, cancellationToken);
        return suggestion is null ? NotFound() : Ok(suggestion);
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept(Guid noteId, AcceptSuggestionRequest request, CancellationToken cancellationToken)
    {
        var result = await suggestionsService.AcceptAsync(CurrentUserId(), noteId, request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Note)
            : result.FailureReason switch
            {
                AcceptSuggestionFailureReason.SuggestionNotFound => NotFound(),
                AcceptSuggestionFailureReason.InvalidCandidateIds => BadRequest(new { error = "One or more candidate IDs are invalid." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
    }

    [HttpPost("ignore")]
    public async Task<IActionResult> Ignore(Guid noteId, CancellationToken cancellationToken)
    {
        var ignored = await suggestionsService.IgnoreAsync(CurrentUserId(), noteId, cancellationToken);
        return ignored ? NoContent() : NotFound();
    }

    // On-demand re-run of AI Categorization (GLOSSARY.md) - distinct from, and doesn't
    // affect, the one-time automatic trigger on first save with content (ticket 025). Lives
    // on INotesService (it needs the Note's own content), called from here since it's a
    // suggestion-lifecycle action from the API's point of view.
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(Guid noteId, CancellationToken cancellationToken)
    {
        var triggered = await notesService.TriggerCategorizationAsync(CurrentUserId(), noteId, cancellationToken);
        return triggered ? Accepted() : NotFound();
    }

    // [Authorize] already guarantees a valid, signed token with a "sub" claim (see
    // AuthController.Me) - Guid.Parse here can only throw on a token-format change,
    // which should fail loudly rather than silently scope a query to the wrong user.
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
