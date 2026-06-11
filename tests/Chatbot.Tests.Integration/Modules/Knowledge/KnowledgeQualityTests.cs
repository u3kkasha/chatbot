using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Chatbot.Modules.Knowledge.Services;
using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.Ai.Models;
using Chatbot.Shared.Brokers.Processing;
using Chatbot.Shared.Brokers.Vectors;
using Chatbot.Shared.Infrastructure.Configuration;
using Chatbot.Tests.Integration.Infrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Integration.Modules.Knowledge;

public class KnowledgeQualityTests : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly IContainer _doclingContainer;
    private readonly IContainer _qdrantContainer;
    private string? _apiKey;

    public KnowledgeQualityTests(IntegrationTestWebApplicationFactory factory)
    {
        _factory = factory;

        _doclingContainer = new ContainerBuilder("quay.io/docling-project/docling-serve:latest")
            .WithPortBinding(5001, true)
            .WithEnvironment("DOCLING_SERVE_ENABLE_UI", "false")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(5001)))
            .Build();

        _qdrantContainer = new ContainerBuilder("qdrant/qdrant:latest")
            .WithPortBinding(6333, true)
            .WithPortBinding(6334, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        _apiKey = Environment.GetEnvironmentVariable("AI__ApiKey");

        await Task.WhenAll(
            _doclingContainer.StartAsync(),
            _qdrantContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _doclingContainer.DisposeAsync().AsTask(),
            _qdrantContainer.DisposeAsync().AsTask()
        );
    }

    [LiveFact("AI__ApiKey")]
    [Trait("Category", "Live")]
    public async Task RAG_ShouldRetrieveSyntheticFactFromMarkdown_ProofOfQuality()
    {
        // 1. ARRANGE
        var tenantId = Guid.NewGuid().ToString();
        var needleFact = @"
# Protocol Midnight-Alpha
**Required Action**: Execute a `VORTEX-GHOST-7` sequence.
**Authorized By**: General Xylos.
**Priority**: Ultra-Critical
";
        var haystack = @"
# Microwave Oven Safety
Always ensure the door is closed before operating. Do not place metal objects inside.

# Gardening Tips for Spring
Plant your tomatoes after the last frost. Ensure they get at least 6 hours of sunlight.

# Local Weather Report
Expect scattered showers throughout the afternoon with a high of 65 degrees.
";

        // Setup Live Services
        var aiOptions = GetLiveAiOptions();
        var aiBroker = CreateLiveAiBroker(aiOptions);
        var doclingClient = CreateLiveDoclingClient();
        var processingBroker = new ProcessingBroker(doclingClient);
        var qdrantClient = CreateLiveQdrantClient();
        var qdrantBroker = new QdrantBroker(qdrantClient);
        var sparseVectorService = new SparseVectorService();

        var knowledgeFoundationService = new KnowledgeFoundationService(
            aiBroker,
            sparseVectorService,
            qdrantBroker,
            _factory.Services.GetRequiredService<Chatbot.Shared.Brokers.Logging.ILoggingBroker>()
        );

        // 2. ACT - Ingestion (Using the high-level Broker)
        await qdrantBroker.CreateHybridCollectionIfNotExistsAsync("knowledge", 4096);

        // Process Needle
        await IngestTextAsync(needleFact, "needle.md", "needle-001");

        // Process Haystack
        await IngestTextAsync(haystack, "haystack.md", "haystack-001");

        // 3. ACT - Retrieval
        var query = "What is the required action for Midnight-Alpha protocol?";
        var resultsOneOf = await knowledgeFoundationService.RetrieveAsync(query);

        // 4. ASSERT
        resultsOneOf.IsT0.ShouldBeTrue("Retrieval should succeed");
        var results = resultsOneOf.AsT0;

        results.ShouldNotBeEmpty();

        // Quality Log
        foreach (var (r, i) in results.Select((r, i) => (r, i)))
        {
            Console.WriteLine($"Result [{i}] (Score: {r.Score}): {r.Content.Replace("\n", " ")}");
        }

        results.Any(r => r.Content.Contains("VORTEX-GHOST-7")).ShouldBeTrue("The fact should be in the retrieved results");

        // We expect the specific action chunk to be in the top 3 at least
        var top3Contents = string.Join(" | ", results.Take(3).Select(r => r.Content));
        top3Contents.ShouldContain("VORTEX-GHOST-7");

        // Verify Rank 1 is from the needle document
        results[0].DocumentId.ShouldBe("needle-001");

        // Inner Helper: Ingestion
        async Task IngestTextAsync(string text, string fileName, string docId)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var chunks = await processingBroker.ChunkDocumentAsync(stream, fileName);

            var hybridPoints = new List<QdrantHybridPoint>();
            foreach (var chunk in chunks)
            {
                var denseVector = await aiBroker.GenerateEmbeddingAsync(chunk.Content);
                var sparseVector = sparseVectorService.GenerateAsync(chunk.Content);

                hybridPoints.Add(new QdrantHybridPoint(
                    Content: chunk.Content,
                    DenseVector: denseVector,
                    SparseVector: sparseVector,
                    Payload: new Dictionary<string, object?>
                    {
                        ["documentId"] = docId,
                        ["tenantId"] = tenantId
                    }
                ));
            }

            await qdrantBroker.UpsertHybridVectorsAsync("knowledge", hybridPoints);
        }
    }

    private AiOptions GetLiveAiOptions()
    {
        return new AiOptions
        {
            ApiKey = _apiKey!,
            Endpoint = Environment.GetEnvironmentVariable("AI__Endpoint") ?? "https://openrouter.ai/api/v1",
            ModelId = Environment.GetEnvironmentVariable("AI__ModelId") ?? "openai/gpt-4o-mini",
            EmbeddingModelId = Environment.GetEnvironmentVariable("AI__EmbeddingModelId") ?? "qwen/qwen3-embedding-8b",
            RerankModelId = Environment.GetEnvironmentVariable("AI__RerankModelId") ?? "cohere/rerank-v3.5"
        };
    }

    private IAiBroker CreateLiveAiBroker(AiOptions options)
    {
        var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(options.ApiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(options.Endpoint)
        });

        var chatClient = openAiClient.GetChatClient(options.ModelId).AsIChatClient();
        var embeddingGenerator = openAiClient.GetEmbeddingClient(options.EmbeddingModelId).AsIEmbeddingGenerator();

        var httpClient = new HttpClient { BaseAddress = new Uri(options.Endpoint.TrimEnd('/') + "/") };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
        var openRouterClient = new OpenRouterClient(httpClient);

        return new AiBroker(chatClient, embeddingGenerator, openRouterClient, Options.Create(options));
    }

    private IDoclingClient CreateLiveDoclingClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://{_doclingContainer.Hostname}:{_doclingContainer.GetMappedPublicPort(5001)}")
        };
        return new DoclingClient(httpClient);
    }

    private Qdrant.Client.QdrantClient CreateLiveQdrantClient()
    {
        return new Qdrant.Client.QdrantClient(
            _qdrantContainer.Hostname,
            _qdrantContainer.GetMappedPublicPort(6334)
        );
    }
}
