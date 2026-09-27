using Jotwai.Domain.Interfaces;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Jotwai.Controllers;

// Content Polish (GLOSSARY.md, ticket 041) - stateless: takes whatever Markdown the client
// currently has (saved or not), returns a polished version. Nothing is persisted here; the
// client decides whether to keep the result via its own preview/confirm step.
[Authorize]
[ApiController]
[Route("api/content-polish")]
public sealed class ContentPolishController(IContentPolishService contentPolishService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Polish(PolishContentRequest request, CancellationToken cancellationToken)
    {
        var polished = await contentPolishService.PolishAsync(request.Content, cancellationToken);
        return Ok(new PolishContentResponse { Content = polished });
    }
}
