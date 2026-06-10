using System.Collections.Generic;
using System.Linq;
using Chatbot.Shared.Brokers.Ai;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.Extensions.Options;

namespace Chatbot.Tests.Integration;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    public IntegrationTestWebApplicationFactory()
    {
        System.Environment.SetEnvironmentVariable("AI__Endpoint", "https://api.openai.com/v1");
        System.Environment.SetEnvironmentVariable("AI__ApiKey", "test-key");
        System.Environment.SetEnvironmentVariable("AI__ModelId", "gpt-4");
        System.Environment.SetEnvironmentVariable("AI__EmbeddingModelId", "text-embedding-3-small");
        System.Environment.SetEnvironmentVariable("AI__RerankModelId", "cohere/rerank-v3.5");
        System.Environment.SetEnvironmentVariable("Processing__BaseUrl", "http://localhost:5000");
        System.Environment.SetEnvironmentVariable("Qdrant__Host", "localhost");
        System.Environment.SetEnvironmentVariable("Qdrant__Port", "6334");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "AI:Endpoint", "https://api.openai.com/v1" },
                { "AI:ApiKey", "test-key" },
                { "AI:ModelId", "gpt-4" },
                { "AI:EmbeddingModelId", "text-embedding-3-small" },
                { "AI:RerankModelId", "cohere/rerank-v3.5" },
                { "Processing:BaseUrl", "http://localhost:5000" },
                { "Qdrant:Host", "localhost" },
                { "Qdrant:Port", "6334" }
            };
            config.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureTestServices(services =>
        {
            // Inject valid options directly
            services.Replace(ServiceDescriptor.Singleton<IOptions<Chatbot.Shared.Infrastructure.Configuration.AiOptions>>(
                Options.Create(new Chatbot.Shared.Infrastructure.Configuration.AiOptions
                {
                    Endpoint = "https://api.openai.com/v1",
                    ApiKey = "test-key",
                    ModelId = "gpt-4",
                    EmbeddingModelId = "text-embedding-3-small",
                    RerankModelId = "cohere/rerank-v3.5"
                })
            ));

            services.Replace(ServiceDescriptor.Singleton<IOptions<Chatbot.Shared.Infrastructure.Configuration.ProcessingOptions>>(
                Options.Create(new Chatbot.Shared.Infrastructure.Configuration.ProcessingOptions
                {
                    BaseUrl = "http://localhost:5000"
                })
            ));

            services.Replace(ServiceDescriptor.Singleton<IOptions<Chatbot.Shared.Infrastructure.Configuration.QdrantOptions>>(
                Options.Create(new Chatbot.Shared.Infrastructure.Configuration.QdrantOptions
                {
                    Host = "localhost",
                    Port = 6334
                })
            ));

            // Replace AI brokers with Noop implementations to avoid hitting real AI services
            // and bypass missing configuration errors (e.g. ApiKey, Endpoint).
            services.Replace(ServiceDescriptor.Singleton<IChatClient, NoopChatClient>());
            services.Replace(ServiceDescriptor.Singleton<IEmbeddingGenerator<string, Embedding<float>>, NoopEmbeddingGenerator>());
        });
    }
}
