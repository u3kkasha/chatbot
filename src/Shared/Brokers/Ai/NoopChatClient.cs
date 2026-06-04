using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Chatbot.Shared.Brokers.Ai;

public class NoopChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new ChatClientMetadata("NoopChatClient");

    public void Dispose() { }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "No AI configured."))
        );
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            Contents = { new TextContent("No AI configured (streaming).") },
        };
        await Task.CompletedTask;
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType == typeof(ChatClientMetadata) ? Metadata : null;
    }
}
