using System.Text.Json.Serialization;
using Jotwai.Domain.Enums;

namespace Jotwai.Domain.ViewModels;

public sealed class NoteSuggestionResponse
{
    public required Guid Id { get; init; }
    public required Guid NoteId { get; init; }

    // Scoped to just this field (not a global JsonSerializerOptions change) - "Placement" over
    // the wire is far less error-prone for the client than a bare int.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required NoteSuggestionKind Kind { get; init; }
    public required Guid TargetNoteId { get; init; }
    public required string TargetNoteName { get; init; }
    public required double Confidence { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
