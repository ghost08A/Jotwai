namespace Jotwai.Domain.Enums;

/// <summary>Note Suggestion's outcome (CONTEXT.md). Only Placement exists so far - Merge is added
/// with ticket 5/7, and will always win over Placement when both apply to the same Note.</summary>
public enum NoteSuggestionKind
{
    Placement,
}
