using System.Collections.Generic;
using System.Threading;

namespace Chatbot.Modules.Chat.Features.StreamCompletion;

/// <summary>
/// Foundation service responsible for orchestrating AI token streaming
/// and projecting broker updates into typed SSE tokens.
/// </summary>
public interface IStreamCompletionService
{
    IAsyncEnumerable<SseToken> StreamAsync(
        string prompt,
        CancellationToken cancellationToken = default
    );
}
