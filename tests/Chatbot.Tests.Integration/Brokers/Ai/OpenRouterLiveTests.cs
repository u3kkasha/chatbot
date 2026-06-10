using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.Ai.Models;
using Chatbot.Shared.Infrastructure.Configuration;
using Chatbot.Tests.Integration.Infrastructure;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenAI;
using Shouldly;

namespace Chatbot.Tests.Integration.Brokers.Ai;

public class OpenRouterLiveTests
{
    private readonly string _apiKey;
    private readonly AiOptions _aiOptions;

    public OpenRouterLiveTests()
    {
        _apiKey = Environment.GetEnvironmentVariable("AI__ApiKey") ?? "";

        _aiOptions = new AiOptions
        {
            ApiKey = _apiKey,
            Endpoint = Environment.GetEnvironmentVariable("AI__Endpoint") ?? "https://openrouter.ai/api/v1",
            ModelId = Environment.GetEnvironmentVariable("AI__ModelId") ?? "google/gemini-2.0-flash-lite-preview-02-05:free",
            EmbeddingModelId = Environment.GetEnvironmentVariable("AI__EmbeddingModelId") ?? "qwen/qwen-turbo",
            RerankModelId = Environment.GetEnvironmentVariable("AI__RerankModelId") ?? "cohere/rerank-v3.5"
        };
    }

    [LiveFact("AI__ApiKey")]
    [Trait("Category", "Live")]
    public async Task ShouldGenerateChatCompletionAsync()
    {
        // given
        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(_apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(_aiOptions.Endpoint)
        });

        IChatClient chatClient = openAiClient.GetChatClient(_aiOptions.ModelId).AsIChatClient();

        var aiBroker = new AiBroker(
            chatClient,
            Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>(),
            Substitute.For<IOpenRouterClient>(),
            Options.Create(_aiOptions));

        // when
        string result = await aiBroker.GetChatCompletionAsync("Say 'Hello World'");

        // then
        result.ShouldNotBeNullOrEmpty();
        result.ToLower().ShouldContain("hello");
    }

    [LiveFact("AI__ApiKey")]
    [Trait("Category", "Live")]
    public async Task ShouldGenerateEmbeddingAsync()
    {
        // given
        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(_apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(_aiOptions.Endpoint)
        });

        var embeddingGenerator = openAiClient.GetEmbeddingClient(_aiOptions.EmbeddingModelId).AsIEmbeddingGenerator();

        var aiBroker = new AiBroker(
            Substitute.For<IChatClient>(),
            embeddingGenerator,
            Substitute.For<IOpenRouterClient>(),
            Options.Create(_aiOptions));

        // when
        float[] result = await aiBroker.GenerateEmbeddingAsync("Hello World");

        // then
        result.ShouldNotBeEmpty();
        result.Length.ShouldBeGreaterThan(0);
    }

    [LiveFact("AI__ApiKey")]
    [Trait("Category", "Live")]
    public async Task ShouldRerankAsync()
    {
        // given
        var httpClient = new HttpClient { BaseAddress = new Uri(_aiOptions.Endpoint.TrimEnd('/') + "/") };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        var openRouterClient = new OpenRouterClient(httpClient);

        var aiBroker = new AiBroker(
            Substitute.For<IChatClient>(),
            Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>(),
            openRouterClient,
            Options.Create(_aiOptions));

        var query = "What is the capital of France?";
        var documents = new[]
        {
            "London is the capital of England.",
            "Paris is the capital of France.",
            "Berlin is the capital of Germany."
        };

        // when
        var results = await aiBroker.RerankAsync(query, documents);

        // then
        results.ShouldNotBeEmpty();
        results.First().Document.ShouldContain("Paris");
    }
}
