using System.Net;
using System.Net.Http.Json;
using Chatbot.Tests.Integration.Brokers.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Chatbot.Tests.Integration;

public class StartupTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public StartupTests(TestDatabaseFixture fixture, IntegrationTestWebApplicationFactory factory)
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
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseData = await response.Content.ReadFromJsonAsync<Chatbot.Api.Infrastructure.Serialization.HealthCheckResponse>();
        responseData.ShouldNotBeNull();
        responseData.Status.ShouldBe("Healthy");
    }

    [Fact]
    public async Task OpenApiDocument_ContainsExpectedMetadata()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/openapi/openapi.json");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonNode>();
        json.ShouldNotBeNull();

        var info = json["info"];
        info.ShouldNotBeNull();

        info["title"]?.ToString().ShouldBe("Omnichannel Chatbot API");
        info["version"]?.ToString().ShouldBe("1.0.0");
        info["description"]?.ToString().ShouldBe("API for the Omnichannel Customer Support Operator Platform.");
    }

    [Fact]
    public async Task ScalarDocumentation_IsAccessible()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/scalar/openapi");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
