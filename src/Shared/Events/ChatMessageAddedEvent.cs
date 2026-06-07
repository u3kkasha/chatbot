using System;
using Chatbot.Shared.Models;
using NodaTime;

namespace Chatbot.Shared.Events;

public record ChatMessageAddedEvent(
    Guid MessageId,
    Guid SessionId,
    TenantId TenantId,
    string Sender,
    string Content,
    bool IsAiGenerated,
    Instant CreatedDate
);
