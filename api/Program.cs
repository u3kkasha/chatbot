using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Register a mock chat client for now
builder.Services.AddSingleton<IChatClient>(
    new SampleChatClient(new Uri("http://localhost"), "mock-model")
);

var app = builder.Build();

app.MapPost(
    "/api/chat",
    async (ChatRequest request, IChatClient chatClient, HttpContext context) =>
    {
        var messages = request
            .Messages.Select(m => new ChatMessage(
                m.Role.Equals("user", StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.User
                    : ChatRole.Assistant,
                m.Content
            ))
            .ToList();

        context.Response.ContentType = "text/event-stream";

        // Set up Vercel AI SDK compatible stream format
        // 0: means "text part"

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
        {
            if (update.Text is not null)
            {
                var chunk = $"0:\"{update.Text.Replace("\"", "\\\"").Replace("\n", "\\n")}\"\n";
                await context.Response.WriteAsync(chunk);
                await context.Response.Body.FlushAsync();
            }
        }
    }
);

app.Run();

public class ChatRequest
{
    public List<MessageDto> Messages { get; set; } = new();
}

public class MessageDto
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
}

public sealed class SampleChatClient(Uri endpoint, string modelId) : IChatClient
{
    public ChatClientMetadata Metadata { get; } = new(nameof(SampleChatClient), endpoint, modelId);

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(300, cancellationToken);
        return new(new ChatMessage(ChatRole.Assistant, "This is a mock response."));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        string[] words =
        [
            "Hello! ",
            "I ",
            "am ",
            "a ",
            "streaming ",
            "mock ",
            "backend ",
            "built ",
            "with ",
            ".NET ",
            "10 ",
            "and ",
            "Microsoft.Extensions.AI!",
        ];
        foreach (string word in words)
        {
            await Task.Delay(150, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, word);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey) => this;

    public TService? GetService<TService>(object? key = null)
        where TService : class => this as TService;

    void IDisposable.Dispose() { }
}
