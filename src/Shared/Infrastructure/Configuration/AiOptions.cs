using System.ComponentModel.DataAnnotations;

namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    [Required]
    public string Endpoint { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string ModelId { get; init; } = string.Empty;
}
