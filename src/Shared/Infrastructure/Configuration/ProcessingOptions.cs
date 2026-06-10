namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class ProcessingOptions
{
    public const string SectionName = "Processing";

    public string BaseUrl { get; init; } = string.Empty;
}
