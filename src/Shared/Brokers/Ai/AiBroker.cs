using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chatbot.Shared.Brokers.Ai.Models;
using Chatbot.Shared.Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Chatbot.Shared.Brokers.Ai;

public class AiBroker(
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IOpenRouterClient openRouterClient,
    IOptions<AiOptions> aiOptions
) : IAiBroker
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

    public async ValueTask<float[]> GenerateEmbeddingAsync(string text)
    {
        var response = await embeddingGenerator.GenerateAsync([text]);
        return response.First().Vector.ToArray();
    }

    public async ValueTask<IReadOnlyList<RerankResult>> RerankAsync(
        string query,
        IEnumerable<string> documents,
        int? topN = null
    )
    {
        var docsList = documents.ToList();
        if (docsList.Count == 0) return [];

        var request = new RerankRequest(
            Model: aiOptions.Value.RerankModelId,
            Query: query,
            Documents: docsList,
            TopN: topN
        );

        var response = await openRouterClient.RerankAsync(request);

        return response.Results
            .Select(r => new RerankResult(r.Index, docsList[r.Index], r.RelevanceScore))
            .ToList();
    }
}
