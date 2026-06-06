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
        MessageSender sender,
        string content,
        Instant createdDate,
        Instant updatedDate
    )
    {
        Id = id;
        SessionId = sessionId;
        Sender = sender;
        Content = content;
        CreatedDate = createdDate;
        UpdatedDate = updatedDate;
    }

    public ChatMessageId Id { get; init; }
    public ChatSessionId SessionId { get; init; }
    public MessageSender Sender { get; init; }
    public string Content { get; init; }
    public List<Citation> Citations { get; init; } = [];
    public AiMetadata? AiMetadata { get; set; }
    public Instant CreatedDate { get; set; }
    public Instant UpdatedDate { get; set; }
}
