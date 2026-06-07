using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Features.Messages;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Modules.Chat.Models.Sessions.Exceptions;
using Chatbot.Shared.Infrastructure.Errors;
using Chatbot.Shared.Brokers.Events;
using Chatbot.Shared.Events;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NodaTime;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Chat.Messages;

public class ChatMessageServiceTests
{
    private readonly IStorageBroker _storageBrokerMock;
    private readonly IEventBroker _eventBrokerMock;
    private readonly IChatMessageService _sut;

    public ChatMessageServiceTests()
    {
        _storageBrokerMock = Substitute.For<IStorageBroker>();
        _eventBrokerMock = Substitute.For<IEventBroker>();
        _sut = new ChatMessageService(_storageBrokerMock, _eventBrokerMock);
    }

    private static ChatMessage CreateRandomChatMessage() =>
        new(
            id: new ChatMessageId(Guid.NewGuid()),
            sessionId: new ChatSessionId(Guid.NewGuid()),
            tenantId: new TenantId(Guid.NewGuid()),
            sender: MessageSender.Customer,
            content: "Hello, this is a test message.",
            status: MessageStatus.Sent,
            isAiGenerated: false,
            approvedBy: null,
            createdDate: Instant.FromUnixTimeSeconds(1000),
            updatedDate: Instant.FromUnixTimeSeconds(1000)
        );

    [Fact]
    public async Task ShouldAddChatMessageAsync()
    {
        // given
        var inputMessage = CreateRandomChatMessage();
        var expectedMessage = inputMessage;
        _storageBrokerMock.InsertChatMessageAsync(inputMessage).Returns(expectedMessage);

        // when
        var result = await _sut.AddChatMessageAsync(inputMessage);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(expectedMessage);
        await _storageBrokerMock.Received(1).InsertChatMessageAsync(inputMessage);
        await _eventBrokerMock.Received(1).PublishAsync(Arg.Any<ChatMessageAddedEvent>());
    }

    [Fact]
    public async Task ShouldRetrieveChatMessageByIdAsync()
    {
        // given
        var expectedMessage = CreateRandomChatMessage();
        var messageId = expectedMessage.Id;
        _storageBrokerMock.SelectChatMessageByIdAsync(messageId).Returns(expectedMessage);

        // when
        var result = await _sut.RetrieveChatMessageByIdAsync(messageId);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(expectedMessage);
        await _storageBrokerMock.Received(1).SelectChatMessageByIdAsync(messageId);
    }

    [Fact]
    public void ShouldRetrieveAllChatMessages()
    {
        // given
        var messages = new[] { CreateRandomChatMessage(), CreateRandomChatMessage() }.AsQueryable();
        _storageBrokerMock.SelectAllChatMessages().Returns(messages);

        // when
        var result = _sut.RetrieveAllChatMessages();

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ToList().ShouldBe(messages.ToList());
        _storageBrokerMock.Received(1).SelectAllChatMessages();
    }

    [Fact]
    public async Task ShouldModifyChatMessageAsync()
    {
        // given
        var existingMessage = CreateRandomChatMessage();
        var modifiedMessage = existingMessage with { Content = "Updated content" };
        _storageBrokerMock.SelectChatMessageByIdAsync(existingMessage.Id).Returns(existingMessage);
        _storageBrokerMock.UpdateChatMessageAsync(modifiedMessage).Returns(modifiedMessage);

        // when
        var result = await _sut.ModifyChatMessageAsync(modifiedMessage);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(modifiedMessage);
        await _storageBrokerMock.Received(1).SelectChatMessageByIdAsync(existingMessage.Id);
        await _storageBrokerMock.Received(1).UpdateChatMessageAsync(modifiedMessage);
    }

    [Fact]
    public async Task ShouldRemoveChatMessageByIdAsync()
    {
        // given
        var existingMessage = CreateRandomChatMessage();
        var messageId = existingMessage.Id;
        _storageBrokerMock.SelectChatMessageByIdAsync(messageId).Returns(existingMessage);
        _storageBrokerMock.DeleteChatMessageAsync(existingMessage).Returns(existingMessage);

        // when
        var result = await _sut.RemoveChatMessageByIdAsync(messageId);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(existingMessage);
        await _storageBrokerMock.Received(1).SelectChatMessageByIdAsync(messageId);
        await _storageBrokerMock.Received(1).DeleteChatMessageAsync(existingMessage);
    }

    // ──────────────────────────────────────────────
    // Validation Errors
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenChatMessageIsNull()
    {
        // given
        ChatMessage? nullMessage = null;

        // when
        var result = await _sut.AddChatMessageAsync(nullMessage!);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Message.ShouldBe("Chat message is null.");
    }

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenIdIsInvalid()
    {
        // given
        var invalidMessage = new ChatMessage(
            id: new ChatMessageId(Guid.Empty),
            sessionId: new ChatSessionId(Guid.NewGuid()),
            tenantId: new TenantId(Guid.NewGuid()),
            sender: MessageSender.Customer,
            content: "Hello",
            status: MessageStatus.Sent,
            isAiGenerated: false,
            approvedBy: null,
            createdDate: default,
            updatedDate: default
        );

        // when
        var result = await _sut.AddChatMessageAsync(invalidMessage);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatMessage.Id)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenSessionIdIsInvalid()
    {
        // given
        var invalidMessage = new ChatMessage(
            id: new ChatMessageId(Guid.NewGuid()),
            sessionId: new ChatSessionId(Guid.Empty),
            tenantId: new TenantId(Guid.NewGuid()),
            sender: MessageSender.Customer,
            content: "Hello",
            status: MessageStatus.Sent,
            isAiGenerated: false,
            approvedBy: null,
            createdDate: default,
            updatedDate: default
        );

        // when
        var result = await _sut.AddChatMessageAsync(invalidMessage);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatMessage.SessionId)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenTenantIdIsInvalid()
    {
        // given
        var invalidMessage = new ChatMessage(
            id: new ChatMessageId(Guid.NewGuid()),
            sessionId: new ChatSessionId(Guid.NewGuid()),
            tenantId: new TenantId(Guid.Empty),
            sender: MessageSender.Customer,
            content: "Hello",
            status: MessageStatus.Sent,
            isAiGenerated: false,
            approvedBy: null,
            createdDate: default,
            updatedDate: default
        );

        // when
        var result = await _sut.AddChatMessageAsync(invalidMessage);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatMessage.TenantId)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShouldReturnValidationErrorOnAdd_WhenContentIsInvalid(string? invalidContent)
    {
        // given
        var invalidMessage = new ChatMessage(
            id: new ChatMessageId(Guid.NewGuid()),
            sessionId: new ChatSessionId(Guid.NewGuid()),
            tenantId: new TenantId(Guid.NewGuid()),
            sender: MessageSender.Customer,
            content: invalidContent!,
            status: MessageStatus.Sent,
            isAiGenerated: false,
            approvedBy: null,
            createdDate: default,
            updatedDate: default
        );

        // when
        var result = await _sut.AddChatMessageAsync(invalidMessage);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatMessage.Content)).ShouldBeTrue();
    }

    // ──────────────────────────────────────────────
    // NotFound Errors
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnNotFoundErrorOnRetrieveById_WhenMessageDoesNotExist()
    {
        // given
        var messageId = new ChatMessageId(Guid.NewGuid());
        _storageBrokerMock.SelectChatMessageByIdAsync(messageId).Returns((ChatMessage?)null);

        // when
        var result = await _sut.RetrieveChatMessageByIdAsync(messageId);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Message.ShouldBe($"Chat message with id '{messageId.Value}' was not found.");
    }

    [Fact]
    public async Task ShouldReturnNotFoundErrorOnModify_WhenMessageDoesNotExist()
    {
        // given
        var message = CreateRandomChatMessage();
        _storageBrokerMock.SelectChatMessageByIdAsync(message.Id).Returns((ChatMessage?)null);

        // when
        var result = await _sut.ModifyChatMessageAsync(message);

        // then
        result.IsT2.ShouldBeTrue();
        result.AsT2.Message.ShouldBe($"Chat message with id '{message.Id.Value}' was not found.");
    }

    [Fact]
    public async Task ShouldReturnNotFoundErrorOnRemove_WhenMessageDoesNotExist()
    {
        // given
        var messageId = new ChatMessageId(Guid.NewGuid());
        _storageBrokerMock.SelectChatMessageByIdAsync(messageId).Returns((ChatMessage?)null);

        // when
        var result = await _sut.RemoveChatMessageByIdAsync(messageId);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Message.ShouldBe($"Chat message with id '{messageId.Value}' was not found.");
    }

    // ──────────────────────────────────────────────
    // Dependency Exceptions
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnAdd_WhenDbUpdateExceptionOccurs()
    {
        // given
        var message = CreateRandomChatMessage();
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.InsertChatMessageAsync(message).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatMessageDependencyException>(() =>
            _sut.AddChatMessageAsync(message).AsTask()
        );
    }

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnRetrieveById_WhenDbExceptionOccurs()
    {
        // given
        var messageId = new ChatMessageId(Guid.NewGuid());
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.SelectChatMessageByIdAsync(messageId).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatMessageDependencyException>(() =>
            _sut.RetrieveChatMessageByIdAsync(messageId).AsTask()
        );
    }

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnModify_WhenDbUpdateExceptionOccurs()
    {
        // given
        var message = CreateRandomChatMessage();
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.SelectChatMessageByIdAsync(message.Id).Returns(message);
        _storageBrokerMock.UpdateChatMessageAsync(message).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatMessageDependencyException>(() =>
            _sut.ModifyChatMessageAsync(message).AsTask()
        );
    }

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnRemove_WhenDbUpdateExceptionOccurs()
    {
        // given
        var message = CreateRandomChatMessage();
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.SelectChatMessageByIdAsync(message.Id).Returns(message);
        _storageBrokerMock.DeleteChatMessageAsync(message).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatMessageDependencyException>(() =>
            _sut.RemoveChatMessageByIdAsync(message.Id).AsTask()
        );
    }

    // ──────────────────────────────────────────────
    // Service Exceptions
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldThrowServiceExceptionOnAdd_WhenUnexpectedExceptionOccurs()
    {
        // given
        var message = CreateRandomChatMessage();
        var unexpectedException = new Exception("Unexpected error");
        _storageBrokerMock.InsertChatMessageAsync(message).Throws(unexpectedException);

        // when / then
        await Should.ThrowAsync<ChatMessageServiceException>(() =>
            _sut.AddChatMessageAsync(message).AsTask()
        );
    }
}
