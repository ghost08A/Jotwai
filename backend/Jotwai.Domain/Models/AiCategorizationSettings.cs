namespace Jotwai.Domain.Models;

/// <summary>
/// Single-row global setting (ticket 035 - DB-backed, not app config, so it can change
/// without a redeploy; no per-user tuning or admin UI yet). <see cref="Id"/> is always 1.
/// </summary>
public sealed class AiCategorizationSettings
{
    public int Id { get; init; }
    public required double ConfidenceThreshold { get; set; }
}
