using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Chatbot.Shared.Brokers.Processing.Models;

public record DocumentChunk(
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("metadata")] Dictionary<string, object?> Metadata);

public record ProcessingResponse(
    [property: JsonPropertyName("chunks")] IReadOnlyList<DocumentChunk> Chunks);
