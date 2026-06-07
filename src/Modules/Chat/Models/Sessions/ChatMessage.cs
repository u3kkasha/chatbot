using System;
using System.Collections.Generic;
using NodaTime;

namespace Chatbot.Modules.Chat.Models.Sessions;

public record ChatMessage
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Required for EF Core.
    // Parameterless constructor for EF Core
    private ChatMessage() { }
#pragma warning restore CS8618

    public ChatMessage(
        ChatMessageId id,
        ChatSessionId sessionId,
        TenantId tenantId,
        MessageSender sender,
        string content,
        MessageStatus status,
        bool isAiGenerated,
        Guid? approvedBy,
        Instant createdDate,
        Instant updatedDate
    )
    {
        Id = id;
        SessionId = sessionId;
        TenantId = tenantId;
        Sender = sender;
        Content = content;
        Status = status;
        IsAiGenerated = isAiGenerated;
        ApprovedBy = approvedBy;
        CreatedDate = createdDate;
        UpdatedDate = updatedDate;
    }

    public ChatMessageId Id { get; init; }
    public ChatSessionId SessionId { get; init; }
    public TenantId TenantId { get; init; }
    public MessageSender Sender { get; init; }
    public string Content { get; init; }
    public MessageStatus Status { get; init; }
    public bool IsAiGenerated { get; init; }
    public Guid? ApprovedBy { get; init; }
    public List<Citation> Citations { get; init; } = [];
    public AiMetadata? AiMetadata { get; set; }
    public Instant CreatedDate { get; set; }
    public Instant UpdatedDate { get; set; }
}

