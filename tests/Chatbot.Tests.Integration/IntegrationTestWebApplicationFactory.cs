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
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                // Disable the Outbox entirely in integration tests.
                // This prevents background polling workers from starting and looking for tables that might not exist yet or connection issues during shutdown.
                { "MassTransit:Outbox:Provider", "None" }
            };
            config.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace AI brokers with Noop implementations to avoid hitting real AI services
            // and bypass missing configuration errors (e.g. ApiKey, Endpoint).
            services.Replace(ServiceDescriptor.Singleton<IChatClient, NoopChatClient>());
            services.Replace(ServiceDescriptor.Singleton<IEmbeddingGenerator<string, Embedding<float>>, NoopEmbeddingGenerator>());
        });
    }
}
