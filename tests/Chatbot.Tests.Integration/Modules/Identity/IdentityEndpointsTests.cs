using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Chatbot.Tests.Integration.Brokers.Storage;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Integration.Modules.Identity;

public class IdentityEndpointsTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public IdentityEndpointsTests(TestDatabaseFixture fixture, IntegrationTestWebApplicationFactory factory)
    {
        _httpClient = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (_, config) => config.AddConfiguration(fixture.Configuration)
                );
            })
            .CreateClient();
    }

    [Fact]
    public async Task GetIdentity_ReturnsIdentityModule()
    {
        // given / when
        var response = await _httpClient.GetAsync("/identity");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("IdentityModule");
    }
}
