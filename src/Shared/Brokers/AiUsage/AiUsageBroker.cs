using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chatbot.Shared.Brokers.AiUsage;

public class AiUsageBroker(ILogger<AiUsageBroker> logger) : IAiUsageBroker
{
    public ValueTask RecordUsageAsync(
        string model,
        int promptTokens,
        int completionTokens,
        string? userId = null
    )
    {
        logger.LogInformation(
            "AI Usage: Model={Model}, PromptTokens={PromptTokens}, CompletionTokens={CompletionTokens}, TotalTokens={TotalTokens}, UserId={UserId}",
            model,
            promptTokens,
            completionTokens,
            promptTokens + completionTokens,
            userId ?? "Anonymous"
        );

        return ValueTask.CompletedTask;
    }
}
