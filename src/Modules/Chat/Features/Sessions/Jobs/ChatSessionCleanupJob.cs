using System.Threading.Tasks;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Shared.Brokers.Logging;
using Coravel.Invocable;

namespace Chatbot.Modules.Chat.Features.Sessions.Jobs;

public class ChatSessionCleanupJob(
    IStorageBroker storageBroker,
    ILoggingBroker loggingBroker)
    : IInvocable
{
    private readonly IStorageBroker storageBroker = storageBroker;
    private readonly ILoggingBroker loggingBroker = loggingBroker;

    public async Task Invoke()
    {
        this.loggingBroker.LogInformation("Running background job: ChatSessionCleanupJob");
        // Simulate cleanup of older inactive sessions
        await Task.CompletedTask;
    }
}
