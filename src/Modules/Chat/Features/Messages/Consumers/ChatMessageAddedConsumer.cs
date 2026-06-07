using System;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Features.Messages;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Brokers.DistributedLock;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Events;
using Chatbot.Shared.Models;
using MassTransit;
using NodaTime;

namespace Chatbot.Modules.Chat.Features.Messages.Consumers;

public class ChatMessageAddedConsumer(
    IChatMessageService messageService,
    IDistributedLockBroker lockBroker,
    ILoggingBroker loggingBroker,
    IClock clock)
    : IConsumer<ChatMessageAddedEvent>
{
    private readonly IChatMessageService messageService = messageService;
    private readonly IDistributedLockBroker lockBroker = lockBroker;
    private readonly ILoggingBroker loggingBroker = loggingBroker;
    private readonly IClock clock = clock;

    public async Task Consume(ConsumeContext<ChatMessageAddedEvent> context)
    {
        var messageEvent = context.Message;

        // Skip AI-generated messages to prevent infinite suggestion loops
        if (messageEvent.IsAiGenerated || messageEvent.Sender == MessageSender.Ai.ToString())
        {
            return;
        }

        string lockKey = $"locks:session:{messageEvent.SessionId}";
        string lockValue = Guid.NewGuid().ToString();
        TimeSpan lockTimeout = TimeSpan.FromSeconds(10);

        this.loggingBroker.LogInformation($"Attempting to acquire lock for session {messageEvent.SessionId}");

        if (await this.lockBroker.AcquireLockAsync(lockKey, lockValue, lockTimeout))
        {
            try
            {
                this.loggingBroker.LogInformation($"Acquired lock for session {messageEvent.SessionId}. Generating automated response...");

                // Simulate processing / suggestion generation
                // In Phase 4, this will invoke Semantic Kernel RAG flow.
                // For now, we add a simulated AI suggested response.
                var suggestionContent = $"[Auto-Suggestion] Received: \"{messageEvent.Content}\". This is a simulated suggestion.";

                var aiMessage = new ChatMessage(
                    id: new ChatMessageId(Guid.NewGuid()),
                    sessionId: new ChatSessionId(messageEvent.SessionId),
                    tenantId: messageEvent.TenantId,
                    sender: MessageSender.Ai,
                    content: suggestionContent,
                    status: MessageStatus.Draft, // Suggestions are drafts until operator sends them
                    isAiGenerated: true,
                    approvedBy: null,
                    createdDate: this.clock.GetCurrentInstant(),
                    updatedDate: this.clock.GetCurrentInstant()
                );

                var result = await this.messageService.AddChatMessageAsync(aiMessage);
                if (result.IsT0)
                {
                    this.loggingBroker.LogInformation($"Successfully generated suggestion for session {messageEvent.SessionId}");
                }
                else
                {
                    this.loggingBroker.LogError(new Exception("Failed to save auto-suggestion: " + result.AsT1.Message));
                }
            }
            finally
            {
                await this.lockBroker.ReleaseLockAsync(lockKey, lockValue);
                this.loggingBroker.LogInformation($"Released lock for session {messageEvent.SessionId}");
            }
        }
        else
        {
            this.loggingBroker.LogWarning($"Failed to acquire lock for session {messageEvent.SessionId}. Skipping suggestion generation.");
        }
    }
}
