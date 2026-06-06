using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.ServerSentEvents;
using System.Text.Json;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Features.StreamCompletion;
using Chatbot.Tests.Integration.Brokers.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Chatbot.Tests.Integration.Modules.Chat;

public class SseStreamingTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public SseStreamingTests(TestDatabaseFixture fixture, IntegrationTestWebApplicationFactory factory)
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
    public async Task StreamEndpoint_ReturnsEventStream_ContentType()
    {
        // given / when
        var response = await _httpClient.GetAsync(
            "/api/chat/completions/stream?prompt=hello",
            HttpCompletionOption.ResponseHeadersRead
        );

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");
    }

    [Fact]
    public async Task StreamEndpoint_EmitsTokens_AndFinalSentinel()
    {
        // given
        using var response = await _httpClient.GetAsync(
            "/api/chat/completions/stream?prompt=hello",
            HttpCompletionOption.ResponseHeadersRead
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // when – parse the SSE stream
        var tokens = await ReadAllSseTokensAsync(response);

        // then
        tokens.ShouldNotBeEmpty();
        var last = tokens[^1];
        last.IsFinal.ShouldBeTrue();
    }

    [Fact]
    public async Task StreamEndpoint_ReturnsBadRequest_WhenPromptMissing()
    {
        // given / when
        var response = await _httpClient.GetAsync("/api/chat/completions/stream");

        // then – missing required query parameter
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static async Task<List<SseToken>> ReadAllSseTokensAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        SseItemParser<SseToken?> itemParser = (_, data) =>
            JsonSerializer.Deserialize<SseToken>(data, options);

        var parser = SseParser.Create(stream, itemParser);

        var tokens = new List<SseToken>();
        await foreach (var item in parser.EnumerateAsync())
        {
            if (item.Data is not null)
                tokens.Add(item.Data);
        }

        return tokens;
    }
}
