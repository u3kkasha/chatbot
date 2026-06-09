using System.Linq;
using Chatbot.Shared.Brokers.Ai;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Chatbot.Tests.Integration;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove startup validators so we don't need dummy configuration settings in tests
            var validationDescriptors = services
                .Where(d => d.ServiceType.FullName == "Microsoft.Extensions.Options.IStartupValidator")
                .ToList();

            foreach (var descriptor in validationDescriptors)
            {
                services.Remove(descriptor);
            }

            // Replace AI brokers with Noop implementations to avoid hitting real AI services
            // and bypass missing configuration errors (e.g. ApiKey, Endpoint).
            services.Replace(ServiceDescriptor.Singleton<IChatClient, NoopChatClient>());
            services.Replace(ServiceDescriptor.Singleton<IEmbeddingGenerator<string, Embedding<float>>, NoopEmbeddingGenerator>());
        });
    }
}
