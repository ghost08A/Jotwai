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
        var result = await notesService.CreateAsync(CurrentUserId(), request, cancellationToken);
        return result.Succeeded ? StatusCode(StatusCodes.Status201Created, result.Note) : MapWriteFailure(result);
    }

    [HttpPut("{id:guid}/name")]
    public async Task<IActionResult> Rename(Guid id, RenameNoteRequest request, CancellationToken cancellationToken)
    {
        var result = await notesService.RenameAsync(CurrentUserId(), id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Note) : MapWriteFailure(result);
    }

    [HttpPut("{id:guid}/parent")]
    public async Task<IActionResult> Move(Guid id, MoveNoteRequest request, CancellationToken cancellationToken)
    {
        var result = await notesService.MoveAsync(CurrentUserId(), id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Note) : MapWriteFailure(result);
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

    // The Topic Detail page's continuous-document backing (ticket - "1 Topic = continuous
    // document") - a Topic has at most one active Note (NoteConfiguration's unique filtered
    // index), addressed here by topicId instead of noteId.
    [HttpGet("by-topic/{topicId:guid}")]
    public async Task<IActionResult> GetByTopic(Guid topicId, CancellationToken cancellationToken)
    {
        var note = await notesService.GetByTopicAsync(CurrentUserId(), topicId, cancellationToken);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPut("by-topic/{topicId:guid}")]
    public async Task<IActionResult> UpsertByTopic(Guid topicId, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var result = await notesService.UpsertByTopicAsync(CurrentUserId(), topicId, request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Note)
            : result.FailureReason switch
            {
                UpsertTopicNoteFailureReason.TopicNotFound => NotFound(),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
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
        var result = await notesService.RestoreAsync(CurrentUserId(), id, cancellationToken);
        return result.Succeeded ? Ok(result.Note) : MapWriteFailure(result);
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
                AssignNoteTopicFailureReason.TopicAlreadyHasNote => Conflict(new { error = "This topic already has a note." }),
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

    private IActionResult MapWriteFailure(NoteWriteResult result) => result.FailureReason switch
    {
        NoteWriteFailureReason.ParentNotFound => BadRequest(new { error = "Parent Note not found." }),
        NoteWriteFailureReason.InvalidName => BadRequest(new { error = "Name must be 1-200 characters." }),
        NoteWriteFailureReason.CircularMove => Conflict(new { error = "A Note can't be moved under itself or one of its own sub-notes." }),
        NoteWriteFailureReason.ParentInTrash => Conflict(new { error = "Restore the parent Note first - it is still in the Trash." }),
        NoteWriteFailureReason.NotFound => NotFound(),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };

    // [Authorize] already guarantees a valid, signed token with a "sub" claim (see
    // AuthController.Me) - Guid.Parse here can only throw on a token-format change,
    // which should fail loudly rather than silently scope a query to the wrong user.
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
