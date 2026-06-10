namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 6334;

    public bool UseHttps { get; init; } = false;

    public string? ApiKey { get; init; }
}
