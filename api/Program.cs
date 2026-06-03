using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Register a mock chat client for now
builder.Services.AddSingleton<IChatClient>(
    new SampleChatClient(new Uri("http://localhost"), "mock-model")
);

var app = builder.Build();

var chats = new List<ChatDto>();

app.MapGet("/api/chats", () => chats);

app.MapPost(
    "/api/chats",
    (ChatCreateDto request) =>
    {
        var chat = new ChatDto
        {
            Id = request.Id ?? Guid.NewGuid().ToString(),
            Title = "New Chat",
            CreatedAt = DateTime.UtcNow,
        };
        chats.Add(chat);
        return Results.Created($"/api/chats/{chat.Id}", chat);
    }
);

app.MapGet(
    "/api/chats/{id}",
    (string id) =>
    {
        var chat = chats.FirstOrDefault(c => c.Id == id);
        return chat is not null ? Results.Ok(chat) : Results.NotFound();
    }
);

app.MapPatch(
    "/api/chats/{id}/title",
    (string id, ChatUpdateDto request) =>
    {
        var chat = chats.FirstOrDefault(c => c.Id == id);
        if (chat is null)
            return Results.NotFound();
        chat.Title = request.Title;
        return Results.Ok(chat);
    }
);

app.MapDelete(
    "/api/chats/{id}",
    (string id) =>
    {
        var chat = chats.FirstOrDefault(c => c.Id == id);
        if (chat is null)
            return Results.NotFound();
        chats.Remove(chat);
        return Results.NoContent();
    }
);

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

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();

public class ChatRequest
{
    public List<MessageDto> Messages { get; set; } = new();
}

public class ChatDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ChatCreateDto
{
    public string? Id { get; set; }
}

public class ChatUpdateDto
{
    public string Title { get; set; } = "";
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
