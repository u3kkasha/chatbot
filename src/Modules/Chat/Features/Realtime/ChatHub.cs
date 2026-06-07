using System;
using System.Threading.Tasks;
using Chatbot.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatbot.Modules.Chat.Features.Realtime;

public class ChatHub(ITenantProvider tenantProvider) : Hub
{
    private readonly ITenantProvider tenantProvider = tenantProvider;

    public override async Task OnConnectedAsync()
    {
        var tenantId = this.tenantProvider.GetTenantId();
        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId.Value}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = this.tenantProvider.GetTenantId();
        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant:{tenantId.Value}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
