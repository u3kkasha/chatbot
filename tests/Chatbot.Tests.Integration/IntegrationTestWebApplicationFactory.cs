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
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("MassTransit:Outbox:Provider", "None");

        builder.ConfigureTestServices(services =>
        {
            // Replace AI brokers with Noop implementations to avoid hitting real AI services
            // and bypass missing configuration errors (e.g. ApiKey, Endpoint).
            services.Replace(ServiceDescriptor.Singleton<IChatClient, NoopChatClient>());
            services.Replace(ServiceDescriptor.Singleton<IEmbeddingGenerator<string, Embedding<float>>, NoopEmbeddingGenerator>());
        });
    }
}
