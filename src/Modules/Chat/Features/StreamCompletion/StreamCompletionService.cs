using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Chatbot.Shared.Brokers.Ai;

namespace Chatbot.Modules.Chat.Features.StreamCompletion;

/// <summary>
/// Foundation service that delegates to <see cref="IAiBroker"/> for streaming completions
/// and maps each <c>ChatResponseUpdate</c> into an <see cref="SseToken"/>.
/// </summary>
public class StreamCompletionService(IAiBroker aiBroker) : IStreamCompletionService
{
    public async IAsyncEnumerable<SseToken> StreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var index = 0;

        await foreach (
            var update in aiBroker
                .StreamChatCompletionAsync(prompt)
                .WithCancellation(cancellationToken)
        )
        {
            var text = update.Text ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return new SseToken(text, IsFinal: false);
            }

            index++;
        }

        // Signal stream completion to the client
        yield return new SseToken(string.Empty, IsFinal: true);
    }
}
