using System;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Features.Messages;
using Chatbot.Modules.Chat.Features.Messages.Consumers;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Brokers.DistributedLock;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Events;
using Chatbot.Shared.Models;
using MassTransit;
using NSubstitute;
using NodaTime;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Chat.Messages.Consumers;

public class ChatMessageAddedConsumerTests
{
    private readonly IChatMessageService _messageServiceMock;
    private readonly IDistributedLockBroker _lockBrokerMock;
    private readonly ILoggingBroker _loggingBrokerMock;
    private readonly IClock _clockMock;
    private readonly ChatMessageAddedConsumer _sut;

    public ChatMessageAddedConsumerTests()
    {
        _messageServiceMock = Substitute.For<IChatMessageService>();
        _lockBrokerMock = Substitute.For<IDistributedLockBroker>();
        _loggingBrokerMock = Substitute.For<ILoggingBroker>();
        _clockMock = Substitute.For<IClock>();

        _sut = new ChatMessageAddedConsumer(
            _messageServiceMock,
            _lockBrokerMock,
            _loggingBrokerMock,
            _clockMock
        );
    }

    [Fact]
    public async Task ShouldIgnoreEvent_WhenMessageIsAiGenerated()
    {
        // given
        var @event = new ChatMessageAddedEvent(
            MessageId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            TenantId: new TenantId(Guid.NewGuid()),
            Sender: MessageSender.Ai.ToString(),
            Content: "Hi",
            IsAiGenerated: true,
            CreatedDate: Instant.FromUnixTimeSeconds(100)
        );

        var contextMock = Substitute.For<ConsumeContext<ChatMessageAddedEvent>>();
        contextMock.Message.Returns(@event);

        // when
        await _sut.Consume(contextMock);

        // then
        await _lockBrokerMock.DidNotReceiveWithAnyArgs().AcquireLockAsync(default!, default!, default);
        await _messageServiceMock.DidNotReceiveWithAnyArgs().AddChatMessageAsync(default!);
    }

    [Fact]
    public async Task ShouldGenerateSuggestion_WhenCustomerMessageAndLockAcquired()
    {
        // given
        var sessionId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var @event = new ChatMessageAddedEvent(
            MessageId: Guid.NewGuid(),
            SessionId: sessionId,
            TenantId: tenantId,
            Sender: MessageSender.Customer.ToString(),
            Content: "Help",
            IsAiGenerated: false,
            CreatedDate: Instant.FromUnixTimeSeconds(100)
        );

        var contextMock = Substitute.For<ConsumeContext<ChatMessageAddedEvent>>();
        contextMock.Message.Returns(@event);

        _lockBrokerMock.AcquireLockAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(true);

        _clockMock.GetCurrentInstant().Returns(Instant.FromUnixTimeSeconds(101));

        var expectedSuggestion = new ChatMessage(
            id: new ChatMessageId(Guid.NewGuid()),
            sessionId: new ChatSessionId(sessionId),
            tenantId: tenantId,
            sender: MessageSender.Ai,
            content: "[Auto-Suggestion] Received: \"Help\". This is a simulated suggestion.",
            status: MessageStatus.Draft,
            isAiGenerated: true,
            approvedBy: null,
            createdDate: Instant.FromUnixTimeSeconds(101),
            updatedDate: Instant.FromUnixTimeSeconds(101)
        );

        _messageServiceMock.AddChatMessageAsync(Arg.Any<ChatMessage>())
            .Returns(expectedSuggestion);

        // when
        await _sut.Consume(contextMock);

        // then
        await _lockBrokerMock.Received(1).AcquireLockAsync(
            Arg.Is<string>(k => k == $"locks:session:{sessionId}"),
            Arg.Any<string>(),
            Arg.Is<TimeSpan>(t => t == TimeSpan.FromSeconds(10))
        );

        await _messageServiceMock.Received(1).AddChatMessageAsync(Arg.Is<ChatMessage>(m =>
            m.SessionId.Value == sessionId &&
            m.TenantId == tenantId &&
            m.Sender == MessageSender.Ai &&
            m.Content == "[Auto-Suggestion] Received: \"Help\". This is a simulated suggestion." &&
            m.Status == MessageStatus.Draft &&
            m.IsAiGenerated == true
        ));

        await _lockBrokerMock.Received(1).ReleaseLockAsync(
            Arg.Is<string>(k => k == $"locks:session:{sessionId}"),
            Arg.Any<string>()
        );
    }

    [Fact]
    public async Task ShouldNotGenerateSuggestion_WhenLockNotAcquired()
    {
        // given
        var sessionId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var @event = new ChatMessageAddedEvent(
            MessageId: Guid.NewGuid(),
            SessionId: sessionId,
            TenantId: tenantId,
            Sender: MessageSender.Customer.ToString(),
            Content: "Help",
            IsAiGenerated: false,
            CreatedDate: Instant.FromUnixTimeSeconds(100)
        );

        var contextMock = Substitute.For<ConsumeContext<ChatMessageAddedEvent>>();
        contextMock.Message.Returns(@event);

        _lockBrokerMock.AcquireLockAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(false);

        // when
        await _sut.Consume(contextMock);

        // then
        await _messageServiceMock.DidNotReceiveWithAnyArgs().AddChatMessageAsync(default!);
        await _lockBrokerMock.DidNotReceiveWithAnyArgs().ReleaseLockAsync(default!, default!);
    }
}
