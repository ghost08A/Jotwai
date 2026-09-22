using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Jotwai.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);

        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, result.Tokens)
            : MapFailure(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Tokens)
            : MapFailure(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);

        return result.Succeeded
            ? Ok(result.Tokens)
            : MapFailure(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    // Establishes the Identity Module's auth contract: every future feature (Notes,
    // Organization, AI Categorization, Search) identifies the current user the same
    // way - a valid Bearer access token, its "sub" claim as the user ID.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        // [Authorize] already guarantees a valid, signed token; this only guards against
        // a "sub" claim that isn't a Guid, so a future token-format change fails closed
        // (401) instead of crashing the request (500).
        if (!Guid.TryParse(subject, out var userId))
            return Unauthorized();

        return Ok(new CurrentUserResponse { UserId = userId });
    }

    private IActionResult MapFailure(AuthResult result) => result.FailureReason switch
    {
        AuthFailureReason.EmailAlreadyRegistered => Conflict(new { error = result.Error }),
        AuthFailureReason.PasswordPolicyViolation => BadRequest(new { error = result.Error }),
        AuthFailureReason.InvalidCredentials => Unauthorized(new { error = result.Error }),
        AuthFailureReason.InvalidRefreshToken => Unauthorized(new { error = result.Error }),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };
}
