using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Chatbot.Shared.Brokers.Ai;

public interface IAiBroker
{
    ValueTask<string> GetChatCompletionAsync(
        string prompt,
        IEnumerable<ChatMessage>? history = null
    );
    IAsyncEnumerable<ChatResponseUpdate> StreamChatCompletionAsync(
        string prompt,
        IEnumerable<ChatMessage>? history = null
    );
    ValueTask<float[]> GenerateEmbeddingAsync(string text);
    ValueTask<IReadOnlyList<RerankResult>> RerankAsync(string query, IEnumerable<string> documents, int? topN = null);
}

public record RerankResult(int Index, string Document, float Score);
