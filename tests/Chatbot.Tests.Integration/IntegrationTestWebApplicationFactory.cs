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
        // This ensures the MassTransit Outbox is disabled process-wide for integration tests.
        // We use an environment variable because it is guaranteed to be seen by the configuration
        // builder immediately during the application's startup.
        System.Environment.SetEnvironmentVariable("MassTransit__Outbox__Provider", "None");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // The MassTransit:Outbox:Provider is already set to "None" in the constructor
            // via Environment.SetEnvironmentVariable to ensure it is picked up early.
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
