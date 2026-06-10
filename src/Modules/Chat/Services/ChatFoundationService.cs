using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Services;
using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.Logging;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Chatbot.Modules.Chat.Services;

public class ChatFoundationService(
    IChatClient chatClient,
    IKnowledgeFoundationService knowledgeFoundationService,
    ILoggingBroker loggingBroker
) : IChatFoundationService
{
    public async IAsyncEnumerable<string> ResponseAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var agent = CreateAgent();

        await foreach (var update in agent.RunStreamingAsync(prompt, cancellationToken: cancellationToken))
        {
            if (update.Text is { } text)
            {
                yield return text;
            }
        }
    }

    private AIAgent CreateAgent()
    {
        var searchProvider = new TextSearchProvider(SearchAdapter, new TextSearchProviderOptions
        {
            SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
            RecentMessageMemoryLimit = 5
        });

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "ChatbotAgent",
            AIContextProviders = [searchProvider]
        });
    }

    private async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(
        string query,
        CancellationToken cancellationToken
    )
    {
        var result = await knowledgeFoundationService.RetrieveAsync(query);

        return result.Match(
            chunks => chunks.Select(c => new TextSearchProvider.TextSearchResult
            {
                Text = c.Content,
                SourceName = c.DocumentId,
                SourceLink = null // Link not available in KnowledgeChunk yet
            }),
            serviceError =>
            {
                loggingBroker.LogError(new Exception(serviceError.Message, serviceError.InnerException));
                return Enumerable.Empty<TextSearchProvider.TextSearchResult>();
            },
            dependencyError =>
            {
                loggingBroker.LogError(new Exception(dependencyError.Message, dependencyError.InnerException));
                return Enumerable.Empty<TextSearchProvider.TextSearchResult>();
            }
        );
    }
}
