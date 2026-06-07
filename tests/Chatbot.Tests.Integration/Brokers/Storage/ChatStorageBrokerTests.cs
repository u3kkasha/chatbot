using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Brokers.Storage.CompiledModels;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Integration.Brokers.Storage;

public class ChatStorageBrokerTests(TestDatabaseFixture fixture)
    : IClassFixture<TestDatabaseFixture>
{
    private readonly StorageBroker _storageBroker = new(
        new DbContextOptionsBuilder<StorageBroker>()
            .UseNpgsql(fixture.Configuration.GetConnectionString("DefaultConnection"), x => x.UseNodaTime())
            .UseModel(StorageBrokerModel.Instance)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                new Chatbot.Shared.Infrastructure.Data.AuditInterceptor(SystemClock.Instance),
                new Chatbot.Shared.Infrastructure.Data.RlsInterceptor(NSubstitute.Substitute.For<ITenantProvider>())
            )
            .Options
    );

    [Fact]
    public void StorageBroker_ShouldUseChatSchema()
    {
        // Arrange & Act
        var sessionEntity = _storageBroker.Model.FindEntityType(typeof(ChatSession));
        var messageEntity = _storageBroker.Model.FindEntityType(typeof(ChatMessage));

        // Assert
        sessionEntity?.GetSchema().ShouldBe("chat");
        messageEntity?.GetSchema().ShouldBe("chat");
    }

    [Fact]
    public void StorageBroker_ShouldUseSnakeCaseNamingConvention()
    {
        // Arrange
        var entityType = _storageBroker.Model.FindEntityType(typeof(ChatSession));
        entityType.ShouldNotBeNull();

        // Act
        var tableName = entityType!.GetTableName();
        var idProperty = entityType.FindProperty(nameof(ChatSession.Id));
        var tenantIdProperty = entityType.FindProperty(nameof(ChatSession.TenantId));

        var storeObject = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(
            tableName!,
            entityType.GetSchema()
        );
        var idColumnName = idProperty?.GetColumnName(storeObject);
        var tenantIdColumnName = tenantIdProperty?.GetColumnName(storeObject);

        // Assert
        tableName.ShouldBe("chat_sessions");
        idColumnName.ShouldBe("id");
        tenantIdColumnName.ShouldBe("tenant_id");
    }

    [Fact]
    public async Task InsertChatSessionAsync_ShouldInsertChatSession()
    {
        // Arrange
        await ((DbContext)_storageBroker).Database.EnsureCreatedAsync();

        var now = SystemClock.Instance.GetCurrentInstant();
        var session = new ChatSession(
            Id: new ChatSessionId(Guid.NewGuid()),
            TenantId: new TenantId(Guid.NewGuid()),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: new OperatorId(Guid.NewGuid()),
            Status: ChatSessionStatus.Open,
            CreatedDate: now,
            UpdatedDate: now
        );

        // Act
        var insertedSession = await _storageBroker.InsertChatSessionAsync(session);

        // Assert
        insertedSession.ShouldNotBeNull();
        insertedSession.Id.ShouldBe(session.Id);

        var selectSession = await _storageBroker.SelectChatSessionByIdAsync(session.Id);
        selectSession.ShouldNotBeNull();
        selectSession!.CustomerIdentifier.ShouldBe(session.CustomerIdentifier);
        selectSession.TenantId.ShouldBe(session.TenantId);
    }

    [Fact]
    public async Task InsertChatMessageAsync_ShouldInsertChatMessageAndMapJson()
    {
        // Arrange
        await ((DbContext)_storageBroker).Database.EnsureCreatedAsync();

        var now = SystemClock.Instance.GetCurrentInstant();
        var sessionId = new ChatSessionId(Guid.NewGuid());
        var messageId = new ChatMessageId(Guid.NewGuid());

        var citations = new List<Citation>
        {
            new(
                "https://example.com/doc1",
                "Document 1",
                "This is the first citation snippet",
                0.95
            ),
            new(
                "https://example.com/doc2",
                "Document 2",
                "This is the second citation snippet",
                0.88
            ),
        };

        var aiMetadata = new AiMetadata(
            ModelName: "gpt-4o",
            PromptTokens: 150,
            CompletionTokens: 300,
            TotalTokens: 450,
            LatencyMs: 1200.5
        );

        var message = new ChatMessage(
            id: messageId,
            sessionId: sessionId,
            tenantId: new TenantId(Guid.NewGuid()),
            sender: MessageSender.Ai,
            content: "Hello! Here is the answer you requested.",
            status: MessageStatus.Sent,
            isAiGenerated: true,
            approvedBy: null,
            createdDate: now,
            updatedDate: now
        )
        {
            Citations = citations,
            AiMetadata = aiMetadata,
        };

        // Act
        var insertedMessage = await _storageBroker.InsertChatMessageAsync(message);

        // Assert
        insertedMessage.ShouldNotBeNull();
        insertedMessage.Id.ShouldBe(message.Id);

        var selectMessage = await _storageBroker.SelectChatMessageByIdAsync(message.Id);
        selectMessage.ShouldNotBeNull();
        selectMessage!.Content.ShouldBe(message.Content);
        selectMessage.IsAiGenerated.ShouldBeTrue();
        selectMessage.Citations.Count.ShouldBe(2);
        selectMessage.Citations[0].SourceUrl.ShouldBe("https://example.com/doc1");
        selectMessage.Citations[1].Snippet.ShouldBe("This is the second citation snippet");
        selectMessage.AiMetadata.ShouldNotBeNull();
        selectMessage.AiMetadata!.ModelName.ShouldBe("gpt-4o");
        selectMessage.AiMetadata.TotalTokens.ShouldBe(450);
    }
}
