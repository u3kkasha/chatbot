using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Api.Infrastructure.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(HealthCheckResponse))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }

public record HealthCheckResponse(string Status, DateTime Timestamp);
