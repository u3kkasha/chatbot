using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Chatbot.Shared.Brokers.Ai;

public class AiBroker(IChatClient chatClient) : IAiBroker
{
    public async ValueTask<string> GetChatCompletionAsync(
        string prompt,
        IEnumerable<ChatMessage>? history = null
    )
    {
        var messages = history?.ToList() ?? new List<ChatMessage>();
        messages.Add(new ChatMessage(ChatRole.User, prompt));

        var response = await chatClient.GetResponseAsync(messages);

        return response.Text;
    }

    public IAsyncEnumerable<ChatResponseUpdate> StreamChatCompletionAsync(
        string prompt,
        IEnumerable<ChatMessage>? history = null
    )
    {
        var messages = history?.ToList() ?? new List<ChatMessage>();
        messages.Add(new ChatMessage(ChatRole.User, prompt));

        return chatClient.GetStreamingResponseAsync(messages);
    }
}
