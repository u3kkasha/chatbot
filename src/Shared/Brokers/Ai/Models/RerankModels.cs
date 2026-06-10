using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Chatbot.Shared.Brokers.Ai.Models;

public record RerankRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("documents")] IEnumerable<string> Documents,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: JsonPropertyName("top_n")] int? TopN = null);

public record RerankResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("results")] IReadOnlyList<RerankResponseResult> Results);

public record RerankResponseResult(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("relevance_score")] float RelevanceScore);
