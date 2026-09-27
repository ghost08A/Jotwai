using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jotwai.Domain.Enums;
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

    public async Task<AiTopicSuggestion?> AnalyzeTopicAsync(
        string newTopicName, IReadOnlyList<AiTopicCandidate> existingTopics, CancellationToken cancellationToken = default)
    {
        if (existingTopics.Count == 0)
            return null;

        var gemini = options.Value;
        var prompt = BuildTopicPrompt(newTopicName, existingTopics);

        var requestBody = new GeminiRequest
        {
            Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
            GenerationConfig = new GeminiGenerationConfig
            {
                ResponseMimeType = "application/json",
                ResponseSchema = TopicResponseSchema,
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
            logger.LogWarning("Gemini response had no content candidates for Topic Suggestion.");
            return null;
        }

        var result = JsonSerializer.Deserialize<GeminiTopicSuggestion>(text, JsonOptions);
        if (result is null || string.Equals(result.Kind, "none", StringComparison.OrdinalIgnoreCase) || result.TopicId is null)
            return null;

        if (!Guid.TryParse(result.TopicId, out var matchedTopicId) || existingTopics.All(t => t.TopicId != matchedTopicId))
            return null;

        var kind = result.Kind.Trim().ToLowerInvariant() switch
        {
            "merge" => TopicSuggestionKind.Merge,
            "placement" => TopicSuggestionKind.Placement,
            _ => (TopicSuggestionKind?)null,
        };

        return kind is null ? null : new AiTopicSuggestion(kind.Value, matchedTopicId, Math.Clamp(result.Confidence, 0, 1));
    }

    private static string BuildTopicPrompt(string newTopicName, IReadOnlyList<AiTopicCandidate> existingTopics)
    {
        var existing = string.Join("\n", existingTopics.Select(t => $"- {t.TopicId}: {t.Name}"));

        return $"""
            You are organizing a personal note-taking app's hierarchy of Topics (folders). A
            new Topic was just created. Decide if it should be:
            - "merge": it means essentially the same thing as one of the existing Topics
              below (even if spelled differently), so it should be merged into that one.
            - "placement": it isn't a duplicate, but it reads like it should be a subtopic
              (child folder) of one of the existing Topics below.
            - "none": neither applies - it stands on its own.

            Prefer "none" unless you are quite confident - "merge" is a destructive action
            (the new Topic gets deleted) so only choose it when the name is clearly the same
            concept as an existing one.

            New Topic name: {newTopicName}

            Existing Topics (id: name):
            {existing}

            Respond with the matching existing Topic's id when kind is "merge" or
            "placement", and a confidence between 0 and 1. When kind is "none", topicId can
            be null and confidence should be 0.
            """;
    }

    private static readonly GeminiSchema TopicResponseSchema = new()
    {
        Type = "OBJECT",
        Properties = new Dictionary<string, GeminiSchema>
        {
            ["kind"] = new() { Type = "STRING" },
            ["topicId"] = new() { Type = "STRING" },
            ["confidence"] = new() { Type = "NUMBER" },
        },
        Required = ["kind", "confidence"],
    };

    private sealed class GeminiTopicSuggestion
    {
        [JsonPropertyName("kind")]
        public string Kind { get; init; } = "none";

        [JsonPropertyName("topicId")]
        public string? TopicId { get; init; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; init; }
    }

    public async Task<string> PolishNoteContentAsync(string content, CancellationToken cancellationToken = default)
    {
        var gemini = options.Value;
        var prompt = BuildPolishPrompt(content);

        var requestBody = new GeminiRequest
        {
            Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
            GenerationConfig = new GeminiGenerationConfig { ResponseMimeType = "text/plain" },
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
            logger.LogWarning("Gemini response had no content candidates for Content Polish.");
            return content;
        }

        return text;
    }

    private static string BuildPolishPrompt(string content) => $"""
        You are helping polish a personal note written in Markdown. Improve readability
        without changing the user's original meaning.

        Rules you MUST follow exactly:
        1. Fix typos, misspellings, and grammar mistakes DIRECTLY, in place, with NO
           parentheses and no marking of any kind - just silently correct the spelling. These
           are keystroke-level mistakes in ordinary words, e.g. "sentance" -> "sentence",
           "teh" -> "the", "recieve" -> "receive", "your" used where "you're" was clearly
           meant. A misspelled word is NEVER covered by rule 3 below, even if the correctly
           spelled word looks different - fixing a typo does not change what the sentence
           claims, so it never needs the original kept alongside it.
        2. Reformat long paragraphs into bullet lists where it improves readability - do not
           mark these either.
        3. This rule is ONLY about numbers, dates, names, or factual/mathematical claims that
           could plausibly be intentional even though they look wrong (e.g. "1+1=3", a date
           like "2026-02-30", "Paris is the capital of Germany") - i.e. cases where fixing it
           would change WHAT the sentence asserts, not just how a word is spelled. For these
           cases only: insert your suggested correction and keep the original right after it
           in parentheses, like this: "corrected text (เดิม: original text)". Do not apply
           this rule to ordinary spelling mistakes - see rule 1.
        4. If you add new sentences that elaborate on the content and were not written by the
           user at all, mark them clearly, e.g.: "new elaboration sentence (เพิ่มโดย AI)".
        5. Any line matching "%%JOTWAI_CODE_BLOCK_<number>%%" is a placeholder for a code
           block - reproduce it completely unchanged, do not remove, translate, or alter it.
        6. Preserve the Markdown structure (headings, lists, blockquotes) unless rule 2 asks
           you to restructure a paragraph into a list.

        Return ONLY the polished Markdown content, nothing else - no commentary, no code
        fences wrapping the whole response.

        Content:
        {content}
        """;

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
        public GeminiSchema? ResponseSchema { get; init; }
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
