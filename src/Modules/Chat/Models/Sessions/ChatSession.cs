using NodaTime;

namespace Chatbot.Modules.Chat.Models.Sessions;

public record ChatSession(
    ChatSessionId Id,
    CustomerId CustomerId,
    OperatorId? OperatorId,
    ChatSessionStatus Status,
    Instant CreatedDate,
    Instant UpdatedDate
);
