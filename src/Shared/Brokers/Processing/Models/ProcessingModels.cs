using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Chatbot.Shared.Brokers.Processing.Models;

public record DocumentChunk(
    [property: JsonPropertyName("text")] string Content,
    [property: JsonPropertyName("label")] string Label);

public record DoclingDocument(
    [property: JsonPropertyName("texts")] IReadOnlyList<DocumentChunk> Texts);

public record DoclingConvertResponse(
    [property: JsonPropertyName("json_content")] DoclingDocument? JsonContent,
    [property: JsonPropertyName("md_content")] string? MdContent);

public record ProcessingResponse(
    [property: JsonPropertyName("document")] DoclingConvertResponse Document);
