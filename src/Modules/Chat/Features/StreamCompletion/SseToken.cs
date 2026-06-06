namespace Chatbot.Modules.Chat.Features.StreamCompletion;

/// <summary>
/// Represents a single AI token chunk emitted over an SSE stream.
/// </summary>
/// <param name="Text">The token text fragment.</param>
/// <param name="IsFinal">True when this is the last token in the stream.</param>
public record SseToken(string Text, bool IsFinal = false);
