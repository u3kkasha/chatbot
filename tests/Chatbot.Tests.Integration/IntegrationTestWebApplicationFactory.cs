using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

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
        });
    }
}
