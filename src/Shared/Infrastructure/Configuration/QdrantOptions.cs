using System.ComponentModel.DataAnnotations;

namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    [Required]
    public string Host { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 6334;

    public bool UseHttps { get; init; } = false;

    public string? ApiKey { get; init; }
}
