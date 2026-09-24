namespace Jotwai.Service;

public sealed class GeminiOptions
{
    public required string Model { get; init; }

    /// <summary>Never put a real value in appsettings.Development.json (it's tracked in
    /// git) - set this via `dotnet user-secrets set "Gemini:ApiKey" "..."` in Web.Jotwai
    /// for local dev, or an environment variable in other environments.</summary>
    public required string ApiKey { get; init; }
}
