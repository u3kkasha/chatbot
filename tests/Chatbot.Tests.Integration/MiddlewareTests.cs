using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Chatbot.Tests.Integration.Brokers.Storage;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chatbot.Tests.Integration;

public class MiddlewareTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestDatabaseFixture _fixture;

    public MiddlewareTests(TestDatabaseFixture fixture, WebApplicationFactory<Program> factory)
    {
        _fixture = fixture;
        _factory = factory;
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
    public async Task CorrelationIdMiddleware_ShouldGenerateCorrelationId_WhenMissing()
    {
        // Arrange & Act
        var response = await _httpClient.GetAsync("/health");

        // Assert
        response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ShouldPropagateCorrelationId_WhenProvided()
    {
        // Arrange
        var customCorrelationId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", customCorrelationId);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        correlationId.Should().Be(customCorrelationId);
    }

    private class ExceptionThrowingStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);

                app.Use(
                    async (context, nextMiddleware) =>
                    {
                        if (context.Request.Path == "/throw-test-exception")
                        {
                            throw new InvalidOperationException(
                                "Test exception for GlobalExceptionHandler"
                            );
                        }
                        await nextMiddleware();
                    }
                );
            };
        }
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldReturnProblemDetails_OnUnhandledException()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, config) =>
                    {
                        config.AddConfiguration(_fixture.Configuration);
                    }
                );
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IStartupFilter, ExceptionThrowingStartupFilter>();
                });
            })
            .CreateClient();

        // Act
        var response = await client.GetAsync("/throw-test-exception");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Internal Server Error");
        problemDetails.Status.Should().Be(500);
        problemDetails.Detail.Should().NotBeNullOrWhiteSpace();
    }
}
