using System.Text.Json.Serialization;
using Jotwai.Domain.Enums;

namespace Jotwai.Domain.ViewModels;

public sealed class TopicSuggestionResponse
{
    public required Guid Id { get; init; }
    public required Guid TopicId { get; init; }

    // Scoped to just this field (not a global JsonSerializerOptions change) - "Merge"/
    // "Placement" over the wire is far less error-prone for the client than a bare int.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TopicSuggestionKind Kind { get; init; }
    public required Guid TargetTopicId { get; init; }
    public required string TargetTopicName { get; init; }
    public required double Confidence { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
