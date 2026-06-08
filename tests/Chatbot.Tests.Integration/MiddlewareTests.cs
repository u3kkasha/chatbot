using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Chatbot.Tests.Integration.Brokers.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Integration;

public class MiddlewareTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly TestDatabaseFixture _fixture;

    public MiddlewareTests(TestDatabaseFixture fixture, IntegrationTestWebApplicationFactory factory)
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
        response.Headers.Contains("X-Correlation-Id").ShouldBeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        Guid.TryParse(correlationId, out _).ShouldBeTrue();
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
        response.Headers.Contains("X-Correlation-Id").ShouldBeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        correlationId.ShouldBe(customCorrelationId);
    }

    private class HeaderAddingStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    context.Response.Headers.Append("Server", "TestServer");
                    context.Response.Headers.Append("X-Powered-By", "TestPower");
                    await nextMiddleware();
                });
                next(app);
            };
        }
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_ShouldRemoveSensitiveHeaders()
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
                    services.AddTransient<IStartupFilter, HeaderAddingStartupFilter>();
                });
            })
            .CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.Headers.Contains("Server").ShouldBeFalse();
        response.Headers.Contains("X-Powered-By").ShouldBeFalse();
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
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails!.Title.ShouldBe("Internal Server Error");
        problemDetails.Status.ShouldBe(500);
        problemDetails.Detail.ShouldNotBeNullOrWhiteSpace();
    }
}
