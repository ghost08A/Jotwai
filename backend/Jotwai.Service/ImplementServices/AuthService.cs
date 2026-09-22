using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Jotwai.Service.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class AuthService(
    IdentityDbContext db,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService) : IAuthService
{
    // A fixed, precomputed hash to verify against when no real user is found, so an
    // unknown-email login costs exactly the same as a wrong-password login - not just
    // an identical response body, but an identical amount of work. Computed once per
    // process (not per request) since PasswordHasher's own iteration count already
    // makes the verify itself the expensive, timing-relevant part.
    private static readonly User DummyUser = new()
    {
        Id = Guid.Empty,
        Email = "no-such-user@example.invalid",
        PasswordHash = string.Empty,
        CreatedAt = DateTimeOffset.UnixEpoch,
    };

    private static readonly string DummyPasswordHash =
        new PasswordHasher<User>().HashPassword(DummyUser, "not-a-real-password-Ab1!");

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);

        var violations = PasswordPolicy.Validate(request.Password);
        if (violations.Count > 0)
            return AuthResult.Failure(AuthFailureReason.PasswordPolicyViolation, string.Join(" ", violations));

        var alreadyRegistered = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (alreadyRegistered)
            return AuthResult.Failure(AuthFailureReason.EmailAlreadyRegistered, "This email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = string.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);

        var tokens = IssueTokens(user);

        await db.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(tokens);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Always verifies - against the real hash if the user exists, against the fixed
        // dummy hash otherwise - so an unknown email can't be told apart from a wrong
        // password by either the response or how long it took to produce.
        var verified = passwordHasher.VerifyHashedPassword(
            user ?? DummyUser,
            user?.PasswordHash ?? DummyPasswordHash,
            request.Password) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;

        if (user is null || !verified)
            return AuthResult.Failure(AuthFailureReason.InvalidCredentials, "Invalid email or password.");

        var tokens = IssueTokens(user);
        await db.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(tokens);
    }

    public async Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var existing = await db.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existing is null || existing.Revoked || existing.ExpiresAt <= DateTimeOffset.UtcNow)
            return AuthResult.Failure(AuthFailureReason.InvalidRefreshToken, "Invalid or expired refresh token.");

        // Atomic compare-and-swap at the database, not a tracked-entity write flushed later:
        // only the first of any concurrent replay of this token can flip Revoked from false
        // to true. A losing replay affects zero rows and is rejected outright.
        var rotated = await db.RefreshTokens
            .Where(t => t.Id == existing.Id && !t.Revoked)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Revoked, true), cancellationToken);

        if (rotated == 0)
            return AuthResult.Failure(AuthFailureReason.InvalidRefreshToken, "Invalid or expired refresh token.");

        var user = await db.Users.SingleAsync(u => u.Id == existing.UserId, cancellationToken);
        var tokens = IssueTokens(user);

        await db.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(tokens);
    }

    private TokenResponse IssueTokens(User user)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            Revoked = false,
        });

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.RawToken,
            ExpiresIn = tokenService.AccessTokenExpiresInSeconds,
        };
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
