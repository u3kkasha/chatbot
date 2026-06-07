using NodaTime;

namespace Chatbot.Modules.Chat.Models.Sessions;

public record ChatSession(
    ChatSessionId Id,
    TenantId TenantId,
    ChannelProvider ChannelProvider,
    string? ExternalReferenceId,
    string CustomerIdentifier,
    OperatorId? OperatorId,
    ChatSessionStatus Status,
    Instant CreatedDate,
    Instant UpdatedDate
);
