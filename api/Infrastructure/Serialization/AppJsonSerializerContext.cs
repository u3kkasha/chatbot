using System.Text.Json.Serialization;

namespace Chatbot.Api.Infrastructure.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(HealthCheckResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }

public record HealthCheckResponse(string Status, DateTime Timestamp);
