using System.Text.RegularExpressions;
using Jotwai.Domain.Interfaces;

namespace Jotwai.Service.ImplementServices;

/// <summary>
/// Content Polish (CONTEXT.md, ticket 041). Fenced code blocks are never sent to the AI
/// provider (ground rule: code content is never touched) - they're swapped for opaque
/// placeholder lines before the call and restored verbatim from the original text afterwards.
/// </summary>
public sealed partial class ContentPolishService(IAiProvider aiProvider) : IContentPolishService
{
    private const string PlaceholderPrefix = "%%JOTWAI_CODE_BLOCK_";
    private const string PlaceholderSuffix = "%%";

    public async Task<string> PolishAsync(string content, CancellationToken cancellationToken = default)
    {
        var codeBlocks = new List<string>();
        var withPlaceholders = CodeBlockPattern().Replace(content, match =>
        {
            var placeholder = $"{PlaceholderPrefix}{codeBlocks.Count}{PlaceholderSuffix}";
            codeBlocks.Add(match.Value);
            return placeholder;
        });

        var polished = await aiProvider.PolishNoteContentAsync(withPlaceholders, cancellationToken);

        for (var i = 0; i < codeBlocks.Count; i++)
            polished = polished.Replace($"{PlaceholderPrefix}{i}{PlaceholderSuffix}", codeBlocks[i]);

        return polished;
    }

    [GeneratedRegex(@"```[\s\S]*?```", RegexOptions.Multiline)]
    private static partial Regex CodeBlockPattern();
}
