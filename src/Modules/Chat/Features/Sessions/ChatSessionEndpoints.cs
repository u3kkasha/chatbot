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

namespace Chatbot.Modules.Chat.Features.Sessions;

public record CreateChatSessionRequest(
    Guid TenantId,
    string ChannelProvider,
    string? ExternalReferenceId,
    string CustomerIdentifier,
    Guid? OperatorId
);

public record UpdateChatSessionRequest(
    Guid TenantId,
    string ChannelProvider,
    string? ExternalReferenceId,
    string CustomerIdentifier,
    Guid? OperatorId,
    string Status
);

public record ChatSessionResponse(
    Guid Id,
    Guid TenantId,
    string ChannelProvider,
    string? ExternalReferenceId,
    string CustomerIdentifier,
    Guid? OperatorId,
    string Status,
    Instant CreatedDate,
    Instant UpdatedDate
);

public static class ChatSessionEndpoints
{
    public static IEndpointRouteBuilder MapChatSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/chat/sessions");

        group.MapPost("/", CreateSessionAsync)
            .WithName("CreateChatSession")
            .WithSummary("Create a new chat session.");

        group.MapGet("/", GetAllSessions)
            .WithName("GetAllChatSessions")
            .WithSummary("Retrieve all chat sessions.");

        group.MapGet("/{id:guid}", GetSessionByIdAsync)
            .WithName("GetChatSessionById")
            .WithSummary("Retrieve a chat session by ID.");

        group.MapPut("/{id:guid}", UpdateSessionAsync)
            .WithName("UpdateChatSession")
            .WithSummary("Update a chat session.");

        group.MapDelete("/{id:guid}", DeleteSessionAsync)
            .WithName("DeleteChatSession")
            .WithSummary("Delete a chat session.");

        return routes;
    }

    private static async Task<Results<Created<ChatSessionResponse>, ValidationProblem>> CreateSessionAsync(
        CreateChatSessionRequest request,
        IChatSessionService service
    )
    {
        if (!Enum.TryParse<ChannelProvider>(request.ChannelProvider, true, out var channelProvider))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "channel_provider", ["Invalid channel provider value."] }
            };
            return TypedResults.ValidationProblem(errors, "Invalid channel provider value.");
        }

        var session = new ChatSession(
            Id: ChatSessionId.From(Guid.NewGuid()),
            TenantId: TenantId.From(request.TenantId),
            ChannelProvider: channelProvider,
            ExternalReferenceId: request.ExternalReferenceId,
            CustomerIdentifier: request.CustomerIdentifier,
            OperatorId: request.OperatorId.HasValue ? OperatorId.From(request.OperatorId.Value) : null,
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        );

        var result = await service.AddChatSessionAsync(session);

        return result.Match<Results<Created<ChatSessionResponse>, ValidationProblem>>(
            success => TypedResults.Created($"/api/chat/sessions/{success.Id.Value}", MapToResponse(success)),
            validationError => TypedResults.ValidationProblem(
                errors: validationError.Errors?.ToDictionary(k => k.Key, v => v.Value.ToArray()) ?? new(),
                detail: validationError.Message
            )
        );
    }

    private static Results<Ok<List<ChatSessionResponse>>, ValidationProblem> GetAllSessions(
        IChatSessionService service
    )
    {
        var result = service.RetrieveAllChatSessions();

        return result.Match<Results<Ok<List<ChatSessionResponse>>, ValidationProblem>>(
            sessions => TypedResults.Ok(sessions.Select(MapToResponse).ToList()),
            validationError => TypedResults.ValidationProblem(
                errors: validationError.Errors?.ToDictionary(k => k.Key, v => v.Value.ToArray()) ?? new(),
                detail: validationError.Message
            )
        );
    }

    private static async Task<Results<Ok<ChatSessionResponse>, ProblemHttpResult>> GetSessionByIdAsync(
        Guid id,
        IChatSessionService service
    )
    {
        var result = await service.RetrieveChatSessionByIdAsync(ChatSessionId.From(id));

        return result.Match<Results<Ok<ChatSessionResponse>, ProblemHttpResult>>(
            success => TypedResults.Ok(MapToResponse(success)),
            notFound => TypedResults.Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound)
        );
    }

    private static async Task<Results<Ok<ChatSessionResponse>, ValidationProblem, ProblemHttpResult>> UpdateSessionAsync(
        Guid id,
        UpdateChatSessionRequest request,
        IChatSessionService service
    )
    {
        if (!Enum.TryParse<ChannelProvider>(request.ChannelProvider, true, out var channelProvider))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "channel_provider", ["Invalid channel provider value."] }
            };
            return TypedResults.ValidationProblem(errors, "Invalid channel provider value.");
        }

        if (!Enum.TryParse<ChatSessionStatus>(request.Status, true, out var status))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "status", ["Invalid status value."] }
            };
            return TypedResults.ValidationProblem(errors, "Invalid status value.");
        }

        var session = new ChatSession(
            Id: ChatSessionId.From(id),
            TenantId: TenantId.From(request.TenantId),
            ChannelProvider: channelProvider,
            ExternalReferenceId: request.ExternalReferenceId,
            CustomerIdentifier: request.CustomerIdentifier,
            OperatorId: request.OperatorId.HasValue ? OperatorId.From(request.OperatorId.Value) : null,
            Status: status,
            CreatedDate: default,
            UpdatedDate: default
        );

        var result = await service.ModifyChatSessionAsync(session);

        return result.Match<Results<Ok<ChatSessionResponse>, ValidationProblem, ProblemHttpResult>>(
            success => TypedResults.Ok(MapToResponse(success)),
            validationError => TypedResults.ValidationProblem(
                errors: validationError.Errors?.ToDictionary(k => k.Key, v => v.Value.ToArray()) ?? new(),
                detail: validationError.Message
            ),
            notFound => TypedResults.Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound)
        );
    }

    private static async Task<Results<Ok<ChatSessionResponse>, ProblemHttpResult>> DeleteSessionAsync(
        Guid id,
        IChatSessionService service
    )
    {
        var result = await service.RemoveChatSessionByIdAsync(ChatSessionId.From(id));

        return result.Match<Results<Ok<ChatSessionResponse>, ProblemHttpResult>>(
            success => TypedResults.Ok(MapToResponse(success)),
            notFound => TypedResults.Problem(detail: notFound.Message, statusCode: StatusCodes.Status404NotFound)
        );
    }

    private static ChatSessionResponse MapToResponse(ChatSession session) =>
        new(
            Id: session.Id.Value,
            TenantId: session.TenantId.Value,
            ChannelProvider: session.ChannelProvider.ToString(),
            ExternalReferenceId: session.ExternalReferenceId,
            CustomerIdentifier: session.CustomerIdentifier,
            OperatorId: session.OperatorId?.Value,
            Status: session.Status.ToString(),
            CreatedDate: session.CreatedDate,
            UpdatedDate: session.UpdatedDate
        );
}
