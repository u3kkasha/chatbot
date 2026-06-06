using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Chatbot.Modules.Chat.Features.StreamCompletion;

/// <summary>
/// Exposes the AI token streaming endpoint as a Minimal API route.
/// Route: GET /api/chat/completions/stream?prompt={prompt}
/// </summary>
internal static class StreamCompletionEndpoints
{
    internal static IEndpointRouteBuilder MapStreamCompletion(this IEndpointRouteBuilder routes)
    {
        routes
            .MapGet("/api/chat/completions/stream", Handle)
            .WithName("StreamCompletion")
            .WithSummary("Stream AI completion tokens via Server-Sent Events.")
            .WithDescription(
                "Opens an SSE connection and emits individual AI token chunks "
                    + "as they are produced by the language model. "
                    + "Each event carries a JSON-serialized SseToken payload. "
                    + "A final event with IsFinal=true signals stream completion."
            )
            .WithTags("Chat")
            .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return routes;
    }

    private static async IAsyncEnumerable<SseItem<SseToken>> BuildStream(
        IStreamCompletionService service,
        string prompt,
        [EnumeratorCancellation] CancellationToken ct
    )
    {
        await foreach (var token in service.StreamAsync(prompt, ct))
        {
            yield return new SseItem<SseToken>(token, eventType: "token");
        }
    }

    private static IResult Handle(
        string prompt,
        IStreamCompletionService service,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return TypedResults.BadRequest(
                new { error = "The 'prompt' query parameter is required." }
            );
        }

        return TypedResults.ServerSentEvents(
            BuildStream(service, prompt, cancellationToken)
        );
    }
}
