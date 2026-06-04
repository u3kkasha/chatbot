using System.Threading.Tasks;

namespace Chatbot.Shared.Brokers.AiUsage;

public interface IAiUsageBroker
{
    ValueTask RecordUsageAsync(
        string model,
        int promptTokens,
        int completionTokens,
        string? userId = null
    );
}
