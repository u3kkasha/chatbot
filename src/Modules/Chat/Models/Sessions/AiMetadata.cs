namespace Chatbot.Modules.Chat.Models.Sessions;

public record AiMetadata(
    string ModelName,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    double LatencyMs
);
