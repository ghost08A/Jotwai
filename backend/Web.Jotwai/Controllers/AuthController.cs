using Jotwai.Domain.Interfaces;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
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

    private IActionResult MapFailure(AuthResult result) => result.FailureReason switch
    {
        AuthFailureReason.EmailAlreadyRegistered => Conflict(new { error = result.Error }),
        AuthFailureReason.PasswordPolicyViolation => BadRequest(new { error = result.Error }),
        AuthFailureReason.InvalidCredentials => Unauthorized(new { error = result.Error }),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };
}
