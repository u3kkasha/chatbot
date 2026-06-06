using System.Text.Json.Serialization;
using Chatbot.Modules.Chat.Features.StreamCompletion;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Api.Infrastructure.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(HealthCheckResponse))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(SseToken))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }

public record HealthCheckResponse(string Status, DateTime Timestamp);
