using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Features.Sessions;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Modules.Chat.Models.Sessions.Exceptions;
using Chatbot.Shared.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NodaTime;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Modules.Chat.Sessions;

public class ChatSessionServiceTests
{
    private readonly IStorageBroker _storageBrokerMock;
    private readonly IChatSessionService _sut;

    public ChatSessionServiceTests()
    {
        _storageBrokerMock = Substitute.For<IStorageBroker>();
        _sut = new ChatSessionService(_storageBrokerMock);
    }

    private static ChatSession CreateRandomChatSession() =>
        new(
            Id: new ChatSessionId(Guid.NewGuid()),
            TenantId: new TenantId(Guid.NewGuid()),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: new OperatorId(Guid.NewGuid()),
            Status: ChatSessionStatus.Open,
            CreatedDate: Instant.FromUnixTimeSeconds(1000),
            UpdatedDate: Instant.FromUnixTimeSeconds(1000)
        );

    [Fact]
    public async Task ShouldAddChatSessionAsync()
    {
        // given
        var inputSession = CreateRandomChatSession();
        var expectedSession = inputSession;
        _storageBrokerMock.InsertChatSessionAsync(inputSession).Returns(expectedSession);

        // when
        var result = await _sut.AddChatSessionAsync(inputSession);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(expectedSession);
        await _storageBrokerMock.Received(1).InsertChatSessionAsync(inputSession);
    }

    [Fact]
    public async Task ShouldRetrieveChatSessionByIdAsync()
    {
        // given
        var expectedSession = CreateRandomChatSession();
        var sessionId = expectedSession.Id;
        _storageBrokerMock.SelectChatSessionByIdAsync(sessionId).Returns(expectedSession);

        // when
        var result = await _sut.RetrieveChatSessionByIdAsync(sessionId);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(expectedSession);
        await _storageBrokerMock.Received(1).SelectChatSessionByIdAsync(sessionId);
    }

    [Fact]
    public void ShouldRetrieveAllChatSessions()
    {
        // given
        var sessions = new[] { CreateRandomChatSession(), CreateRandomChatSession() }.AsQueryable();
        _storageBrokerMock.SelectAllChatSessions().Returns(sessions);

        // when
        var result = _sut.RetrieveAllChatSessions();

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ToList().ShouldBe(sessions.ToList());
        _storageBrokerMock.Received(1).SelectAllChatSessions();
    }

    [Fact]
    public async Task ShouldModifyChatSessionAsync()
    {
        // given
        var existingSession = CreateRandomChatSession();
        var modifiedSession = existingSession with { Status = ChatSessionStatus.Pending };
        _storageBrokerMock.SelectChatSessionByIdAsync(existingSession.Id).Returns(existingSession);
        _storageBrokerMock.UpdateChatSessionAsync(modifiedSession).Returns(modifiedSession);

        // when
        var result = await _sut.ModifyChatSessionAsync(modifiedSession);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(modifiedSession);
        await _storageBrokerMock.Received(1).SelectChatSessionByIdAsync(existingSession.Id);
        await _storageBrokerMock.Received(1).UpdateChatSessionAsync(modifiedSession);
    }

    [Fact]
    public async Task ShouldRemoveChatSessionByIdAsync()
    {
        // given
        var existingSession = CreateRandomChatSession();
        var sessionId = existingSession.Id;
        _storageBrokerMock.SelectChatSessionByIdAsync(sessionId).Returns(existingSession);
        _storageBrokerMock.DeleteChatSessionAsync(existingSession).Returns(existingSession);

        // when
        var result = await _sut.RemoveChatSessionByIdAsync(sessionId);

        // then
        result.IsT0.ShouldBeTrue();
        result.AsT0.ShouldBe(existingSession);
        await _storageBrokerMock.Received(1).SelectChatSessionByIdAsync(sessionId);
        await _storageBrokerMock.Received(1).DeleteChatSessionAsync(existingSession);
    }

    [Fact]
    public async Task ShouldBulkUpdateSessionsStatusAsync()
    {
        // given
        var operatorId = new OperatorId(Guid.NewGuid());
        var fromStatus = ChatSessionStatus.Pending;
        var toStatus = ChatSessionStatus.Resolved;
        var expectedCount = 5;

        _storageBrokerMock.UpdateChatSessionsStatusByOperatorAsync(operatorId, fromStatus, toStatus)
            .Returns(expectedCount);

        // when
        var result = await _sut.BulkUpdateSessionsStatusAsync(operatorId, fromStatus, toStatus);

        // then
        result.ShouldBe(expectedCount);
        await _storageBrokerMock.Received(1).UpdateChatSessionsStatusByOperatorAsync(operatorId, fromStatus, toStatus);
    }

    // ──────────────────────────────────────────────
    // Validation Errors
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenChatSessionIsNull()
    {
        // given
        ChatSession? nullSession = null;

        // when
        var result = await _sut.AddChatSessionAsync(nullSession!);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Message.ShouldBe("Chat session is null.");
    }

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenIdIsInvalid()
    {
        // given
        var invalidSession = new ChatSession(
            Id: new ChatSessionId(Guid.Empty),
            TenantId: new TenantId(Guid.NewGuid()),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: null,
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        );

        // when
        var result = await _sut.AddChatSessionAsync(invalidSession);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatSession.Id)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenTenantIdIsInvalid()
    {
        // given
        var invalidSession = new ChatSession(
            Id: new ChatSessionId(Guid.NewGuid()),
            TenantId: new TenantId(Guid.Empty),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: null,
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        );

        // when
        var result = await _sut.AddChatSessionAsync(invalidSession);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatSession.TenantId)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShouldReturnValidationErrorOnAdd_WhenCustomerIdentifierIsInvalid(string? identifier)
    {
        // given
        var invalidSession = new ChatSession(
            Id: new ChatSessionId(Guid.NewGuid()),
            TenantId: new TenantId(Guid.NewGuid()),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: identifier!,
            OperatorId: null,
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        );

        // when
        var result = await _sut.AddChatSessionAsync(invalidSession);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatSession.CustomerIdentifier)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnValidationErrorOnAdd_WhenOperatorIdIsInvalid()
    {
        // given
        var invalidSession = new ChatSession(
            Id: new ChatSessionId(Guid.NewGuid()),
            TenantId: new TenantId(Guid.NewGuid()),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: new OperatorId(Guid.Empty),
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        );

        // when
        var result = await _sut.AddChatSessionAsync(invalidSession);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Errors.ShouldNotBeNull();
        result.AsT1.Errors.ContainsKey(nameof(ChatSession.OperatorId)).ShouldBeTrue();
    }

    // ──────────────────────────────────────────────
    // NotFound Errors
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnNotFoundErrorOnRetrieveById_WhenSessionDoesNotExist()
    {
        // given
        var sessionId = new ChatSessionId(Guid.NewGuid());
        _storageBrokerMock.SelectChatSessionByIdAsync(sessionId).Returns((ChatSession?)null);

        // when
        var result = await _sut.RetrieveChatSessionByIdAsync(sessionId);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Message.ShouldBe($"Chat session with id '{sessionId.Value}' was not found.");
    }

    [Fact]
    public async Task ShouldReturnNotFoundErrorOnModify_WhenSessionDoesNotExist()
    {
        // given
        var session = CreateRandomChatSession();
        _storageBrokerMock.SelectChatSessionByIdAsync(session.Id).Returns((ChatSession?)null);

        // when
        var result = await _sut.ModifyChatSessionAsync(session);

        // then
        result.IsT2.ShouldBeTrue();
        result.AsT2.Message.ShouldBe($"Chat session with id '{session.Id.Value}' was not found.");
    }

    [Fact]
    public async Task ShouldReturnNotFoundErrorOnRemove_WhenSessionDoesNotExist()
    {
        // given
        var sessionId = new ChatSessionId(Guid.NewGuid());
        _storageBrokerMock.SelectChatSessionByIdAsync(sessionId).Returns((ChatSession?)null);

        // when
        var result = await _sut.RemoveChatSessionByIdAsync(sessionId);

        // then
        result.IsT1.ShouldBeTrue();
        result.AsT1.Message.ShouldBe($"Chat session with id '{sessionId.Value}' was not found.");
    }

    // ──────────────────────────────────────────────
    // Dependency Exceptions
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnAdd_WhenDbUpdateExceptionOccurs()
    {
        // given
        var session = CreateRandomChatSession();
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.InsertChatSessionAsync(session).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatSessionDependencyException>(() =>
            _sut.AddChatSessionAsync(session).AsTask()
        );
    }

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnRetrieveById_WhenDbExceptionOccurs()
    {
        // given
        var sessionId = new ChatSessionId(Guid.NewGuid());
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.SelectChatSessionByIdAsync(sessionId).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatSessionDependencyException>(() =>
            _sut.RetrieveChatSessionByIdAsync(sessionId).AsTask()
        );
    }

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnModify_WhenDbUpdateExceptionOccurs()
    {
        // given
        var session = CreateRandomChatSession();
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.SelectChatSessionByIdAsync(session.Id).Returns(session);
        _storageBrokerMock.UpdateChatSessionAsync(session).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatSessionDependencyException>(() =>
            _sut.ModifyChatSessionAsync(session).AsTask()
        );
    }

    [Fact]
    public async Task ShouldThrowDependencyExceptionOnRemove_WhenDbUpdateExceptionOccurs()
    {
        // given
        var session = CreateRandomChatSession();
        var dbException = new DbUpdateException("Database error");
        _storageBrokerMock.SelectChatSessionByIdAsync(session.Id).Returns(session);
        _storageBrokerMock.DeleteChatSessionAsync(session).Throws(dbException);

        // when / then
        await Should.ThrowAsync<ChatSessionDependencyException>(() =>
            _sut.RemoveChatSessionByIdAsync(session.Id).AsTask()
        );
    }

    // ──────────────────────────────────────────────
    // Service Exceptions
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldThrowServiceExceptionOnAdd_WhenUnexpectedExceptionOccurs()
    {
        // given
        var session = CreateRandomChatSession();
        var unexpectedException = new Exception("Unexpected error");
        _storageBrokerMock.InsertChatSessionAsync(session).Throws(unexpectedException);

        // when / then
        await Should.ThrowAsync<ChatSessionServiceException>(() =>
            _sut.AddChatSessionAsync(session).AsTask()
        );
    }
}
