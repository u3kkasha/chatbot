using System;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Features.Realtime;
using Chatbot.Shared.Events;
using Chatbot.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NodaTime;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Chat.Realtime;

public class ChatMessageAddedRealtimeListenerTests
{
    private readonly IHubContext<ChatHub> _hubContextMock;
    private readonly IHubClients _hubClientsMock;
    private readonly IClientProxy _clientProxyMock;
    private readonly ChatMessageAddedRealtimeListener _sut;

    public ChatMessageAddedRealtimeListenerTests()
    {
        _hubContextMock = Substitute.For<IHubContext<ChatHub>>();
        _hubClientsMock = Substitute.For<IHubClients>();
        _clientProxyMock = Substitute.For<IClientProxy>();

        _hubContextMock.Clients.Returns(_hubClientsMock);
        _hubClientsMock.Group(Arg.Any<string>()).Returns(_clientProxyMock);

        _sut = new ChatMessageAddedRealtimeListener(_hubContextMock);
    }

    [Fact]
    public async Task ShouldSendSignalRNotification_WhenChatMessageAddedEventHandled()
    {
        // given
        var messageId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var @event = new ChatMessageAddedEvent(
            MessageId: messageId,
            SessionId: sessionId,
            TenantId: tenantId,
            Sender: "Customer",
            Content: "Hello",
            IsAiGenerated: false,
            CreatedDate: Instant.FromUnixTimeSeconds(100)
        );

        // when
        await _sut.HandleAsync(@event);

        // then
        _hubClientsMock.Received(1).Group($"tenant:{tenantId.Value}");
        await _clientProxyMock.Received(1).SendCoreAsync(
            "ChatMessageAdded",
            Arg.Is<object[]>(args =>
                args.Length == 1 &&
                args[0] != null
            )
        );
    }
}
