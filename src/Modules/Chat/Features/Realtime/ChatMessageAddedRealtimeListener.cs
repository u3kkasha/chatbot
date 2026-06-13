using System.Threading.Tasks;
using Chatbot.Shared.Events;
using Coravel.Events.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Chatbot.Modules.Chat.Features.Realtime;

public class ChatMessageAddedRealtimeListener(IHubContext<ChatHub> hubContext)
    : IListener<ChatMessageAddedEvent>
{
    private readonly IHubContext<ChatHub> hubContext = hubContext;

    public async Task HandleAsync(ChatMessageAddedEvent messageEvent)
    {
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
}
