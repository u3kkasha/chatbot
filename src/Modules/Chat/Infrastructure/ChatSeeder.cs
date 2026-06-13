using System;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Models.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chatbot.Modules.Chat.Infrastructure;

/// <summary>
/// Seeds the database with demo chat sessions and messages on first startup.
/// Only runs when the <c>chat.chat_sessions</c> table is empty.
/// </summary>
public static class ChatSeeder
{
    // Fixed GUIDs so re-seeds are idempotent and frontend state is stable.
    public static readonly Guid TenantId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid OperatorId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private static readonly Guid Session1Id = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid Session2Id = Guid.Parse("30000000-0000-0000-0000-000000000002");
    private static readonly Guid Session3Id = Guid.Parse("30000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var broker = scope.ServiceProvider.GetRequiredService<IStorageBroker>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChatSeederMarker>>();

        // Guard: only seed once
        if (await broker.SelectAllChatSessions().AnyAsync())
        {
            logger.LogDebug("ChatSeeder: data already present, skipping.");
            return;
        }

        logger.LogInformation("ChatSeeder: seeding demo conversations…");

        // ── Sessions ─────────────────────────────────────────────────────────
        var session1 = await broker.InsertChatSessionAsync(new ChatSession(
            Id: new ChatSessionId(Session1Id),
            TenantId: new Shared.Models.TenantId(TenantId),
            ChannelProvider: ChannelProvider.WebWidget,
            ExternalReferenceId: null,
            CustomerIdentifier: "Sarah Jenkins",
            OperatorId: new OperatorId(OperatorId),
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        ));

        var session2 = await broker.InsertChatSessionAsync(new ChatSession(
            Id: new ChatSessionId(Session2Id),
            TenantId: new Shared.Models.TenantId(TenantId),
            ChannelProvider: ChannelProvider.WhatsApp,
            ExternalReferenceId: "+15550198",
            CustomerIdentifier: "Michael Ross",
            OperatorId: new OperatorId(OperatorId),
            Status: ChatSessionStatus.Open,
            CreatedDate: default,
            UpdatedDate: default
        ));

        var session3 = await broker.InsertChatSessionAsync(new ChatSession(
            Id: new ChatSessionId(Session3Id),
            TenantId: new Shared.Models.TenantId(TenantId),
            ChannelProvider: ChannelProvider.Email,
            ExternalReferenceId: "david.chen@example.com",
            CustomerIdentifier: "David Chen",
            OperatorId: null,
            Status: ChatSessionStatus.Resolved,
            CreatedDate: default,
            UpdatedDate: default
        ));

        // ── Messages for session 1 (active conversation) ──────────────────────
        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session1.Id,
            tenantId: session1.TenantId,
            sender: MessageSender.Customer,
            content: "Hi, I'm trying to update my credit card info for my subscription, but every time I hit save, I get an \"Error Code 402\". Can you help?"
        ));

        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session1.Id,
            tenantId: session1.TenantId,
            sender: MessageSender.Operator,
            content: "Hello Sarah! I can certainly help with that. Error 402 usually means there's a temporary block from your bank for online recurring transactions. Let me check your account status real quick."
        ));

        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session1.Id,
            tenantId: session1.TenantId,
            sender: MessageSender.Customer,
            content: "Oh, okay. That's strange, I just used it yesterday. Please let me know what you find."
        ));

        // ── Messages for session 2 ────────────────────────────────────────────
        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session2.Id,
            tenantId: session2.TenantId,
            sender: MessageSender.Customer,
            content: "Can you confirm if my recent order #88921 has shipped yet?"
        ));

        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session2.Id,
            tenantId: session2.TenantId,
            sender: MessageSender.Operator,
            content: "Hi Michael! Let me pull up your order now. One moment please."
        ));

        // ── Messages for session 3 ────────────────────────────────────────────
        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session3.Id,
            tenantId: session3.TenantId,
            sender: MessageSender.Customer,
            content: "Thank you for resolving the issue. I appreciate the quick response."
        ));

        await broker.InsertChatMessageAsync(BuildMessage(
            sessionId: session3.Id,
            tenantId: session3.TenantId,
            sender: MessageSender.Operator,
            content: "Happy to help, David! Feel free to reach out if anything else comes up.",
            isAiGenerated: true
        ));

        logger.LogInformation("ChatSeeder: seeded {Count} sessions successfully.", 3);
    }

    private static ChatMessage BuildMessage(
        ChatSessionId sessionId,
        Shared.Models.TenantId tenantId,
        MessageSender sender,
        string content,
        bool isAiGenerated = false
    ) =>
        new(
            id: new ChatMessageId(Guid.NewGuid()),
            sessionId: sessionId,
            tenantId: tenantId,
            sender: sender,
            content: content,
            status: MessageStatus.Sent,
            isAiGenerated: isAiGenerated,
            approvedBy: null,
            createdDate: default,
            updatedDate: default
        );
}

/// <summary>Marker type for ILogger category resolution.</summary>
internal sealed class ChatSeederMarker;
