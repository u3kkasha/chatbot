using System.ComponentModel.DataAnnotations;

namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class ProcessingOptions
{
    public const string SectionName = "Processing";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;
}
