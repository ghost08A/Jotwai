using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jotwai.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jotwai.Service.AiProviders;

/// <summary>
/// Google Gemini (model configurable via <see cref="GeminiOptions.Model"/>) implementation of
/// <see cref="IAiProvider"/> (ADR-0004).
/// Talks to the plain REST `generateContent` endpoint with structured JSON output, so
/// parsing the result doesn't depend on the model formatting free text consistently.
/// </summary>
public sealed class GeminiAiProvider(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiAiProvider> logger) : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<AiSuggestedCategory>> SuggestCategoriesAsync(
        string noteContent, IReadOnlyList<string> existingCategoryNames, CancellationToken cancellationToken = default)
    {
        var gemini = options.Value;
        var prompt = BuildPrompt(noteContent, existingCategoryNames);

        var requestBody = new GeminiRequest
        {
            Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
            GenerationConfig = new GeminiGenerationConfig
            {
                ResponseMimeType = "application/json",
                ResponseSchema = ResponseSchema,
            },
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"v1beta/models/{gemini.Model}:generateContent?key={gemini.ApiKey}", requestBody, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Gemini request failed with {StatusCode}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gemini request failed with status {response.StatusCode}.");
        }

        var payload = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);
        var text = payload?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning("Gemini response had no content candidates.");
            return [];
        }

        var candidates = JsonSerializer.Deserialize<List<GeminiCategorySuggestion>>(text, JsonOptions) ?? [];

        return candidates
            .Where(c => !string.IsNullOrWhiteSpace(c.CategoryName))
            .Select(c => new AiSuggestedCategory(c.CategoryName.Trim(), Math.Clamp(c.Confidence, 0, 1)))
            .OrderByDescending(c => c.Confidence)
            .ToList();
    }

    private static string BuildPrompt(string noteContent, IReadOnlyList<string> existingCategoryNames)
    {
        var existing = existingCategoryNames.Count > 0
            ? string.Join(", ", existingCategoryNames)
            : "(none yet)";

        return $"""
            You are categorizing a personal note-taking app's note. Suggest up to 3 ranked
            categories for the note below, most likely first. Prefer reusing one of the
            user's existing categories when it genuinely fits; otherwise propose a short,
            new category name (1-3 words, Title Case).

            Existing categories: {existing}

            Note content:
            {noteContent}
            """;
    }

    private static readonly GeminiSchema ResponseSchema = new()
    {
        Type = "ARRAY",
        Items = new GeminiSchema
        {
            Type = "OBJECT",
            Properties = new Dictionary<string, GeminiSchema>
            {
                ["categoryName"] = new() { Type = "STRING" },
                ["confidence"] = new() { Type = "NUMBER" },
            },
            Required = ["categoryName", "confidence"],
        },
    };

    private sealed class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public required List<GeminiContent> Contents { get; init; }

        [JsonPropertyName("generationConfig")]
        public required GeminiGenerationConfig GenerationConfig { get; init; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public required List<GeminiPart> Parts { get; init; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }
    }

    private sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("responseMimeType")]
        public required string ResponseMimeType { get; init; }

        [JsonPropertyName("responseSchema")]
        public required GeminiSchema ResponseSchema { get; init; }
    }

    private sealed class GeminiSchema
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("items")]
        public GeminiSchema? Items { get; init; }

        [JsonPropertyName("properties")]
        public Dictionary<string, GeminiSchema>? Properties { get; init; }

        [JsonPropertyName("required")]
        public List<string>? Required { get; init; }
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiResponseCandidate>? Candidates { get; init; }
    }

    private sealed class GeminiResponseCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; init; }
    }

    private sealed class GeminiCategorySuggestion
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; init; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; init; }
    }
}
