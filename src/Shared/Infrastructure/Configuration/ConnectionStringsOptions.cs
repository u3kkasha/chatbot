using System.ComponentModel.DataAnnotations;

namespace Chatbot.Shared.Infrastructure.Configuration;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string DefaultConnection { get; init; } = string.Empty;

    [Required]
    public string Redis { get; init; } = string.Empty;

    [Required]
    public string BlobStorage { get; init; } = string.Empty;
}
