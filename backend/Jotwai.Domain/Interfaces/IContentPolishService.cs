namespace Jotwai.Domain.Interfaces;

/// <summary>
/// Content Polish (CONTEXT.md, ticket 041) - stateless, on-demand Markdown rewrite. No
/// persistence: unlike Suggestion, nothing is stored while the caller decides whether to
/// keep the result.
/// </summary>
public interface IContentPolishService
{
    Task<string> PolishAsync(string content, CancellationToken cancellationToken = default);
}
