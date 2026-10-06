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
[Route("api/topics/{topicId:guid}/suggestion")]
public sealed class TopicSuggestionsController(ITopicSuggestionsService topicSuggestionsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive(Guid topicId, CancellationToken cancellationToken)
    {
        var suggestion = await topicSuggestionsService.GetActiveAsync(CurrentUserId(), topicId, cancellationToken);
        return suggestion is null ? NotFound() : Ok(suggestion);
    }

    // Absolute route (not nested under {topicId}) - powers the badge indicator on the Topic
    // Browser page (ticket 040) with one call instead of one per Topic in the tree.
    [HttpGet("/api/topics/suggestions")]
    public async Task<IActionResult> ListActive(CancellationToken cancellationToken)
    {
        var topicIds = await topicSuggestionsService.ListActiveTopicIdsAsync(CurrentUserId(), cancellationToken);
        return Ok(topicIds);
    }

    // Accepts whichever active Topic Suggestion this Topic has (Merge or Placement, see
    // CONTEXT.md) - the client shows its own extra confirm-dialog before calling this for a
    // Merge, since that's the destructive one.
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(Guid topicId, CancellationToken cancellationToken)
    {
        var result = await topicSuggestionsService.AcceptAsync(CurrentUserId(), topicId, cancellationToken);

        return result.Succeeded
            ? NoContent()
            : result.FailureReason switch
            {
                AcceptTopicSuggestionFailureReason.SuggestionNotFound => NotFound(),
                AcceptTopicSuggestionFailureReason.DuplicateSiblingName => Conflict(new { error = "A Topic with this name already exists under the target Topic." }),
                AcceptTopicSuggestionFailureReason.BothTopicsHaveNotes => Conflict(new { error = "Both topics already have notes - merge the content manually first." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
    }

    [HttpPost("ignore")]
    public async Task<IActionResult> Ignore(Guid topicId, CancellationToken cancellationToken)
    {
        var ignored = await topicSuggestionsService.IgnoreAsync(CurrentUserId(), topicId, cancellationToken);
        return ignored ? NoContent() : NotFound();
    }

    // On-demand (or automatic, on leaving the Topic Detail page - ticket 044) re-analysis,
    // using the Topic's *current* name plus whatever content the client currently has -
    // unlike the one-time automatic trigger on Topic creation, this can be called any number
    // of times.
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(Guid topicId, TriggerTopicSuggestionRequest request, CancellationToken cancellationToken)
    {
        var triggered = await topicSuggestionsService.TriggerAsync(CurrentUserId(), topicId, request.Content, cancellationToken);
        return triggered ? NoContent() : NotFound();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
