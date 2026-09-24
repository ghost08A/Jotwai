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
[Route("api/topics")]
public sealed class TopicsController(ITopicsService topicsService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateTopicRequest request, CancellationToken cancellationToken)
    {
        var result = await topicsService.CreateAsync(CurrentUserId(), request, cancellationToken);

        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Topic)
            : MapWriteFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var topics = await topicsService.ListAsync(CurrentUserId(), cancellationToken);
        return Ok(topics);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var topic = await topicsService.GetByIdAsync(CurrentUserId(), id, cancellationToken);
        return topic is null ? NotFound() : Ok(topic);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(Guid id, RenameTopicRequest request, CancellationToken cancellationToken)
    {
        var result = await topicsService.RenameAsync(CurrentUserId(), id, request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Topic)
            : MapWriteFailure(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await topicsService.DeleteAsync(CurrentUserId(), id, cancellationToken);

        return outcome switch
        {
            TopicDeleteOutcome.Deleted => NoContent(),
            TopicDeleteOutcome.NotFound => NotFound(),
            TopicDeleteOutcome.HasChildTopics => Conflict(new { error = "Topic still has child Topics." }),
            TopicDeleteOutcome.HasNotes => Conflict(new { error = "Topic still has Notes." }),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private IActionResult MapWriteFailure(TopicWriteResult result) => result.FailureReason switch
    {
        TopicWriteFailureReason.ParentNotFound => BadRequest(new { error = "Parent Topic not found." }),
        TopicWriteFailureReason.DuplicateSiblingName => Conflict(new { error = "A Topic with this name already exists under the same parent." }),
        TopicWriteFailureReason.NotFound => NotFound(),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };

    // [Authorize] already guarantees a valid, signed token with a "sub" claim (see
    // AuthController.Me) - Guid.Parse here can only throw on a token-format change,
    // which should fail loudly rather than silently scope a query to the wrong user.
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
