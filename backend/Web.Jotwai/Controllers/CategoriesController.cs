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
[Route("api/categories")]
public sealed class CategoriesController(ICategoriesService categoriesService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await categoriesService.CreateAsync(CurrentUserId(), request, cancellationToken);

        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Category)
            : MapWriteFailure(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var categories = await categoriesService.ListAsync(CurrentUserId(), cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await categoriesService.GetByIdAsync(CurrentUserId(), id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(Guid id, RenameCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await categoriesService.RenameAsync(CurrentUserId(), id, request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Category)
            : MapWriteFailure(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await categoriesService.DeleteAsync(CurrentUserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private IActionResult MapWriteFailure(CategoryWriteResult result) => result.FailureReason switch
    {
        CategoryWriteFailureReason.DuplicateName => Conflict(new { error = "A Category with this name already exists." }),
        CategoryWriteFailureReason.NotFound => NotFound(),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };

    // [Authorize] already guarantees a valid, signed token with a "sub" claim (see
    // AuthController.Me) - Guid.Parse here can only throw on a token-format change,
    // which should fail loudly rather than silently scope a query to the wrong user.
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
