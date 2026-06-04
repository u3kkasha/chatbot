using System.Net;
using Chatbot.Tests.Integration.Brokers.Storage;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Tests.Integration;

public class StartupTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public StartupTests(TestDatabaseFixture fixture, WebApplicationFactory<Program> factory)
    {
        _httpClient = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        config.AddConfiguration(fixture.Configuration);
                    }
                );
            })
            .CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task ScalarDocumentation_IsAccessible()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/scalar/v1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
