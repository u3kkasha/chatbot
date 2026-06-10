namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string Endpoint { get; init; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ModelId { get; init; } = string.Empty;

    public string EmbeddingModelId { get; init; } = string.Empty;

    public string RerankModelId { get; init; } = string.Empty;
}
