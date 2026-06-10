namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    public string DefaultConnection { get; init; } = string.Empty;

    public string Redis { get; init; } = string.Empty;

    public string BlobStorage { get; init; } = string.Empty;
}
