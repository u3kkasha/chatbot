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
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(SseToken))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Sessions.CreateChatSessionRequest))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Sessions.UpdateChatSessionRequest))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Sessions.ChatSessionResponse))]
[JsonSerializable(typeof(List<Chatbot.Modules.Chat.Features.Sessions.ChatSessionResponse>))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Messages.CreateChatMessageRequest))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Messages.UpdateChatMessageRequest))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Messages.ChatMessageResponse))]
[JsonSerializable(typeof(List<Chatbot.Modules.Chat.Features.Messages.ChatMessageResponse>))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Messages.CitationDto))]
[JsonSerializable(typeof(Chatbot.Modules.Chat.Features.Messages.AiMetadataDto))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }

public record HealthCheckResponse(string Status, DateTime Timestamp);
