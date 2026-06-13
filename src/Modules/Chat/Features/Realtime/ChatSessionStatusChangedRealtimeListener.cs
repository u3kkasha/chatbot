using System.Threading.Tasks;
using Chatbot.Shared.Events;
using Coravel.Events.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Chatbot.Modules.Chat.Features.Realtime;

public class ChatSessionStatusChangedRealtimeListener(IHubContext<ChatHub> hubContext)
    : IListener<ChatSessionStatusChangedEvent>
{
    private readonly IHubContext<ChatHub> hubContext = hubContext;

    public async Task HandleAsync(ChatSessionStatusChangedEvent sessionEvent)
    {
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
