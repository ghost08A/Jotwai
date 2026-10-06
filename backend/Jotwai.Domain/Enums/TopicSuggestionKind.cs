namespace Jotwai.Domain.Enums;

/// <summary>Topic Suggestion's two mutually exclusive outcomes (CONTEXT.md) - Merge always
/// wins over Placement when both would apply to the same Topic.</summary>
public enum TopicSuggestionKind
{
    Merge,
    Placement,
}
