using System.Threading.Tasks;
using Chatbot.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Chatbot.Modules.Chat.Features.Realtime;

public class RealtimeNotificationConsumer(IHubContext<ChatHub> hubContext)
    : IConsumer<ChatMessageAddedEvent>,
      IConsumer<ChatSessionStatusChangedEvent>
{
    private readonly IHubContext<ChatHub> hubContext = hubContext;

    public async Task Consume(ConsumeContext<ChatMessageAddedEvent> context)
    {
        var messageEvent = context.Message;
        string groupName = $"tenant:{messageEvent.TenantId.Value}";

        await this.hubContext.Clients.Group(groupName).SendAsync("ChatMessageAdded", new
        {
            messageEvent.MessageId,
            messageEvent.SessionId,
            messageEvent.TenantId,
            messageEvent.Sender,
            messageEvent.Content,
            messageEvent.IsAiGenerated,
            messageEvent.CreatedDate
        });
    }

    public async Task Consume(ConsumeContext<ChatSessionStatusChangedEvent> context)
    {
        var sessionEvent = context.Message;
        string groupName = $"tenant:{sessionEvent.TenantId.Value}";

        await this.hubContext.Clients.Group(groupName).SendAsync("ChatSessionStatusChanged", new
        {
            sessionEvent.SessionId,
            sessionEvent.TenantId,
            sessionEvent.OldStatus,
            sessionEvent.NewStatus,
            sessionEvent.UpdatedDate
        });
    }
}
