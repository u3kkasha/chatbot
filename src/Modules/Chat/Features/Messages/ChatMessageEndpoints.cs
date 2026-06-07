using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Infrastructure.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using NodaTime;

namespace Chatbot.Modules.Chat.Features.Messages;

public record CitationDto(string SourceUrl, string Title, string Snippet, double Score);
public record AiMetadataDto(string ModelName, int PromptTokens, int CompletionTokens, int TotalTokens, double LatencyMs);

public record CreateChatMessageRequest(
    Guid SessionId,
    Guid TenantId,
    string Sender,
    string Content,
    string Status,
    bool IsAiGenerated,
    Guid? ApprovedBy,
    List<CitationDto>? Citations,
    AiMetadataDto? AiMetadata
);

public record UpdateChatMessageRequest(
    Guid SessionId,
    Guid TenantId,
    string Sender,
    string Content,
    string Status,
    bool IsAiGenerated,
    Guid? ApprovedBy,
    List<CitationDto>? Citations,
    AiMetadataDto? AiMetadata
);

public record ChatMessageResponse(
    Guid Id,
    Guid SessionId,
    Guid TenantId,
    string Sender,
    string Content,
    string Status,
    bool IsAiGenerated,
    Guid? ApprovedBy,
    List<CitationDto> Citations,
    AiMetadataDto? AiMetadata,
    Instant CreatedDate,
    Instant UpdatedDate
);

public static class ChatMessageEndpoints
{
    public static IEndpointRouteBuilder MapChatMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/chat/messages");

        group.MapPost("/", CreateMessageAsync)
            .WithName("CreateChatMessage")
            .WithSummary("Create a new chat message.");

        group.MapGet("/", GetAllMessages)
            .WithName("GetAllChatMessages")
            .WithSummary("Retrieve all chat messages.");

        group.MapGet("/{id:guid}", GetMessageByIdAsync)
            .WithName("GetChatMessageById")
            .WithSummary("Retrieve a chat message by ID.");

        group.MapPut("/{id:guid}", UpdateMessageAsync)
            .WithName("UpdateChatMessage")
            .WithSummary("Update a chat message.");

        group.MapDelete("/{id:guid}", DeleteMessageAsync)
            .WithName("DeleteChatMessage")
            .WithSummary("Delete a chat message.");

        return routes;
    }

    private static async Task<Results<Created<ChatMessageResponse>, ValidationProblem>> CreateMessageAsync(
        CreateChatMessageRequest request,
        IChatMessageService service
    )
    {
        var validationErrors = new Dictionary<string, string[]>();

        if (!Enum.TryParse<MessageSender>(request.Sender, true, out var sender))
            validationErrors["sender"] = ["Invalid sender value."];

        if (!Enum.TryParse<MessageStatus>(request.Status, true, out var status))
            validationErrors["status"] = ["Invalid status value."];

        if (validationErrors.Count > 0)
            return TypedResults.ValidationProblem(validationErrors);

        var message = new ChatMessage(
            id: ChatMessageId.From(Guid.NewGuid()),
            sessionId: ChatSessionId.From(request.SessionId),
            tenantId: TenantId.From(request.TenantId),
            sender: sender,
            content: request.Content,
            status: status,
            isAiGenerated: request.IsAiGenerated,
            approvedBy: request.ApprovedBy,
            createdDate: default,
            updatedDate: default
        )
        {
            Citations = request.Citations?.Select(c => new Citation(
                c.SourceUrl,
                c.Title,
                c.Snippet,
                c.Score
            )).ToList() ?? [],
            AiMetadata = request.AiMetadata != null ? new AiMetadata(
                request.AiMetadata.ModelName,
                request.AiMetadata.PromptTokens,
                request.AiMetadata.CompletionTokens,
                request.AiMetadata.TotalTokens,
                request.AiMetadata.LatencyMs
            ) : null
        };

        var result = await service.AddChatMessageAsync(message);

        return result.Match<Results<Created<ChatMessageResponse>, ValidationProblem>>(
            success => TypedResults.Created($"/api/chat/messages/{success.Id.Value}", MapToResponse(success)),
            validationError => TypedResults.ValidationProblem(
                errors: validationError.Errors?.ToDictionary(k => k.Key, v => v.Value.ToArray()) ?? new(),
                detail: validationError.Message
            )
        );
    }

    private static Results<Ok<List<ChatMessageResponse>>, ValidationProblem> GetAllMessages(
        IChatMessageService service
    )
    {
        var result = service.RetrieveAllChatMessages();

        return result.Match<Results<Ok<List<ChatMessageResponse>>, ValidationProblem>>(
            messages => TypedResults.Ok(messages.Select(MapToResponse).ToList()),
            validationError => TypedResults.ValidationProblem(
                errors: validationError.Errors?.ToDictionary(k => k.Key, v => v.Value.ToArray()) ?? new(),
                detail: validationError.Message
            )
        );
    }

    private static async Task<Results<Ok<ChatMessageResponse>, ProblemHttpResult>> GetMessageByIdAsync(
        Guid id,
        IChatMessageService service
    )
    {
        var result = await service.RetrieveChatMessageByIdAsync(ChatMessageId.From(id));

        return result.Match<Results<Ok<ChatMessageResponse>, ProblemHttpResult>>(
            success => TypedResults.Ok(MapToResponse(success)),
            notFound => TypedResults.Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound)
        );
    }

    private static async Task<Results<Ok<ChatMessageResponse>, ValidationProblem, ProblemHttpResult>> UpdateMessageAsync(
        Guid id,
        UpdateChatMessageRequest request,
        IChatMessageService service
    )
    {
        var validationErrors = new Dictionary<string, string[]>();

        if (!Enum.TryParse<MessageSender>(request.Sender, true, out var sender))
            validationErrors["sender"] = ["Invalid sender value."];

        if (!Enum.TryParse<MessageStatus>(request.Status, true, out var status))
            validationErrors["status"] = ["Invalid status value."];

        if (validationErrors.Count > 0)
            return TypedResults.ValidationProblem(validationErrors);

        var message = new ChatMessage(
            id: ChatMessageId.From(id),
            sessionId: ChatSessionId.From(request.SessionId),
            tenantId: TenantId.From(request.TenantId),
            sender: sender,
            content: request.Content,
            status: status,
            isAiGenerated: request.IsAiGenerated,
            approvedBy: request.ApprovedBy,
            createdDate: default,
            updatedDate: default
        )
        {
            Citations = request.Citations?.Select(c => new Citation(
                c.SourceUrl,
                c.Title,
                c.Snippet,
                c.Score
            )).ToList() ?? [],
            AiMetadata = request.AiMetadata != null ? new AiMetadata(
                request.AiMetadata.ModelName,
                request.AiMetadata.PromptTokens,
                request.AiMetadata.CompletionTokens,
                request.AiMetadata.TotalTokens,
                request.AiMetadata.LatencyMs
            ) : null
        };

        var result = await service.ModifyChatMessageAsync(message);

        return result.Match<Results<Ok<ChatMessageResponse>, ValidationProblem, ProblemHttpResult>>(
            success => TypedResults.Ok(MapToResponse(success)),
            validationError => TypedResults.ValidationProblem(
                errors: validationError.Errors?.ToDictionary(k => k.Key, v => v.Value.ToArray()) ?? new(),
                detail: validationError.Message
            ),
            notFound => TypedResults.Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound)
        );
    }

    private static async Task<Results<Ok<ChatMessageResponse>, ProblemHttpResult>> DeleteMessageAsync(
        Guid id,
        IChatMessageService service
    )
    {
        var result = await service.RemoveChatMessageByIdAsync(ChatMessageId.From(id));

        return result.Match<Results<Ok<ChatMessageResponse>, ProblemHttpResult>>(
            success => TypedResults.Ok(MapToResponse(success)),
            notFound => TypedResults.Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound)
        );
    }

    private static ChatMessageResponse MapToResponse(ChatMessage message) =>
        new(
            Id: message.Id.Value,
            SessionId: message.SessionId.Value,
            TenantId: message.TenantId.Value,
            Sender: message.Sender.ToString(),
            Content: message.Content,
            Status: message.Status.ToString(),
            IsAiGenerated: message.IsAiGenerated,
            ApprovedBy: message.ApprovedBy,
            Citations: message.Citations.Select(c => new CitationDto(
                c.SourceUrl,
                c.Title,
                c.Snippet,
                c.Score
            )).ToList(),
            AiMetadata: message.AiMetadata != null ? new AiMetadataDto(
                message.AiMetadata.ModelName,
                message.AiMetadata.PromptTokens,
                message.AiMetadata.CompletionTokens,
                message.AiMetadata.TotalTokens,
                message.AiMetadata.LatencyMs
            ) : null,
            CreatedDate: message.CreatedDate,
            UpdatedDate: message.UpdatedDate
        );
}
