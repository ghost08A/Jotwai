namespace Jotwai.Domain.Models;

/// <summary>
/// A freshly generated refresh token: the raw value to hand to the client,
/// and the hash of it that gets persisted - the raw value is never stored.
/// </summary>
public sealed record RefreshTokenIssuance(string RawToken, string TokenHash, DateTimeOffset ExpiresAt);
