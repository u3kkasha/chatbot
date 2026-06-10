using System.Collections.Generic;
using System.Threading;

namespace Chatbot.Modules.Chat.Services;

public interface IChatFoundationService
{
    IAsyncEnumerable<string> ResponseAsync(string prompt, CancellationToken cancellationToken = default);
}
