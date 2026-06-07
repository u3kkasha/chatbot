using System;
using Chatbot.Shared.Models;
using NodaTime;

namespace Chatbot.Shared.Events;

public record ChatSessionStatusChangedEvent(
    Guid SessionId,
    TenantId TenantId,
    string OldStatus,
    string NewStatus,
    Instant UpdatedDate
);
