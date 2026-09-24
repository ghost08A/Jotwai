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
[Route("api/notes")]
public sealed class NotesController(INotesService notesService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateNoteRequest request, CancellationToken cancellationToken)
    {
        var note = await notesService.CreateAsync(CurrentUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, note);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var notes = await notesService.ListAsync(CurrentUserId(), cancellationToken);
        return Ok(notes);
    }

    [HttpGet("trash")]
    public async Task<IActionResult> ListTrash(CancellationToken cancellationToken)
    {
        var notes = await notesService.ListTrashAsync(CurrentUserId(), cancellationToken);
        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var note = await notesService.GetByIdAsync(CurrentUserId(), id, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var note = await notesService.UpdateAsync(CurrentUserId(), id, request, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await notesService.DeleteAsync(CurrentUserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var restored = await notesService.RestoreAsync(CurrentUserId(), id, cancellationToken);
        return restored ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/topic")]
    public async Task<IActionResult> AssignTopic(Guid id, AssignNoteTopicRequest request, CancellationToken cancellationToken)
    {
        var result = await notesService.AssignTopicAsync(CurrentUserId(), id, request.TopicId, cancellationToken);

        return result.Succeeded
            ? Ok(result.Note)
            : result.FailureReason switch
            {
                AssignNoteTopicFailureReason.NoteNotFound => NotFound(),
                AssignNoteTopicFailureReason.TopicNotFound => BadRequest(new { error = "Topic not found." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
    }

    [HttpPost("{id:guid}/categories/{categoryId:guid}")]
    public async Task<IActionResult> AttachCategory(Guid id, Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await notesService.AttachCategoryAsync(CurrentUserId(), id, categoryId, cancellationToken);

        return result.Succeeded
            ? Ok(result.Note)
            : result.FailureReason switch
            {
                NoteCategoryAttachFailureReason.NoteNotFound => NotFound(),
                NoteCategoryAttachFailureReason.CategoryNotFound => BadRequest(new { error = "Category not found." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
    }

    [HttpDelete("{id:guid}/categories/{categoryId:guid}")]
    public async Task<IActionResult> DetachCategory(Guid id, Guid categoryId, CancellationToken cancellationToken)
    {
        var note = await notesService.DetachCategoryAsync(CurrentUserId(), id, categoryId, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    // [Authorize] already guarantees a valid, signed token with a "sub" claim (see
    // AuthController.Me) - Guid.Parse here can only throw on a token-format change,
    // which should fail loudly rather than silently scope a query to the wrong user.
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
