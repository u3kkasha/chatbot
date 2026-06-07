using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Features.Sessions;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Tests.Integration.Brokers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Integration.Modules.Chat;

public class ChatSessionEndpointsTests
    : IClassFixture<TestDatabaseFixture>,
        IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private readonly Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> _configuredFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public ChatSessionEndpointsTests(TestDatabaseFixture fixture, IntegrationTestWebApplicationFactory factory)
    {
        _configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(
                (_, config) => config.AddConfiguration(fixture.Configuration)
            );
        });
        _httpClient = _configuredFactory.CreateClient();
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = _configuredFactory.Services.CreateScope();
        var storageBroker = scope.ServiceProvider.GetRequiredService<Chatbot.Modules.Chat.Brokers.Storage.IStorageBroker>();
        await ((DbContext)storageBroker).Database.EnsureCreatedAsync();
    }

    private static CreateChatSessionRequest MakeCreateRequest(
        Guid? tenantId = null,
        string channelProvider = "WebWidget",
        string customerIdentifier = "customer@example.com",
        Guid? operatorId = null,
        string? externalReferenceId = null
    ) => new(
        TenantId: tenantId ?? Guid.NewGuid(),
        ChannelProvider: channelProvider,
        ExternalReferenceId: externalReferenceId,
        CustomerIdentifier: customerIdentifier,
        OperatorId: operatorId
    );

    [Fact]
    public async Task PostChatSession_ReturnsCreatedAndSession()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var request = MakeCreateRequest(operatorId: Guid.NewGuid());

        // when
        var response = await _httpClient.PostAsJsonAsync("/api/chat/sessions", request, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        session.ShouldNotBeNull();
        session.Id.ShouldNotBe(Guid.Empty);
        session.TenantId.ShouldBe(request.TenantId);
        session.ChannelProvider.ShouldBe(request.ChannelProvider);
        session.CustomerIdentifier.ShouldBe(request.CustomerIdentifier);
        session.OperatorId.ShouldBe(request.OperatorId);
        session.Status.ShouldBe(ChatSessionStatus.Open.ToString());
    }

    [Fact]
    public async Task PostChatSession_ValidationFailure_ReturnsBadRequest()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var request = new CreateChatSessionRequest(
            TenantId: Guid.Empty,
            ChannelProvider: "WebWidget",
            ExternalReferenceId: null,
            CustomerIdentifier: "",
            OperatorId: null
        );

        // when
        var response = await _httpClient.PostAsJsonAsync("/api/chat/sessions", request, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(JsonOptions);
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(400);
        problem.Errors.ShouldNotBeEmpty();
        problem.Errors.ContainsKey("TenantId").ShouldBeTrue();
    }

    [Fact]
    public async Task GetChatSessionById_ReturnsOk()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var createRequest = MakeCreateRequest();
        var createResponse = await _httpClient.PostAsJsonAsync("/api/chat/sessions", createRequest, JsonOptions);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        createdSession.ShouldNotBeNull();

        // when
        var response = await _httpClient.GetAsync($"/api/chat/sessions/{createdSession.Id}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        session.ShouldNotBeNull();
        session.Id.ShouldBe(createdSession.Id);
    }

    [Fact]
    public async Task GetChatSessionById_NotFound_ReturnsNotFound()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var nonExistentId = Guid.NewGuid();

        // when
        var response = await _httpClient.GetAsync($"/api/chat/sessions/{nonExistentId}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(404);
    }

    [Fact]
    public async Task GetAllChatSessions_ReturnsOk()
    {
        // given
        await EnsureDatabaseCreatedAsync();

        // when
        var response = await _httpClient.GetAsync("/api/chat/sessions");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var sessions = await response.Content.ReadFromJsonAsync<List<ChatSessionResponse>>(JsonOptions);
        sessions.ShouldNotBeNull();
    }

    [Fact]
    public async Task PutChatSession_ReturnsOkAndUpdatedSession()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var createRequest = MakeCreateRequest();
        var createResponse = await _httpClient.PostAsJsonAsync("/api/chat/sessions", createRequest, JsonOptions);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        createdSession.ShouldNotBeNull();

        var updateRequest = new UpdateChatSessionRequest(
            TenantId: createdSession.TenantId,
            ChannelProvider: createdSession.ChannelProvider,
            ExternalReferenceId: createdSession.ExternalReferenceId,
            CustomerIdentifier: createdSession.CustomerIdentifier,
            OperatorId: Guid.NewGuid(),
            Status: ChatSessionStatus.Pending.ToString()
        );

        // when
        var response = await _httpClient.PutAsJsonAsync($"/api/chat/sessions/{createdSession.Id}", updateRequest, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedSession = await response.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        updatedSession.ShouldNotBeNull();
        updatedSession.OperatorId.ShouldBe(updateRequest.OperatorId);
        updatedSession.Status.ShouldBe(ChatSessionStatus.Pending.ToString());
    }

    [Fact]
    public async Task PutChatSession_NotFound_ReturnsNotFound()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateChatSessionRequest(
            TenantId: Guid.NewGuid(),
            ChannelProvider: "WebWidget",
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: null,
            Status: ChatSessionStatus.Pending.ToString()
        );

        // when
        var response = await _httpClient.PutAsJsonAsync($"/api/chat/sessions/{nonExistentId}", updateRequest, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteChatSession_ReturnsOkAndDeletedSession()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var createRequest = MakeCreateRequest();
        var createResponse = await _httpClient.PostAsJsonAsync("/api/chat/sessions", createRequest, JsonOptions);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        createdSession.ShouldNotBeNull();

        // when
        var response = await _httpClient.DeleteAsync($"/api/chat/sessions/{createdSession.Id}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deletedSession = await response.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        deletedSession.ShouldNotBeNull();
        deletedSession.Id.ShouldBe(createdSession.Id);
    }

    [Fact]
    public async Task DeleteChatSession_NotFound_ReturnsNotFound()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var nonExistentId = Guid.NewGuid();

        // when
        var response = await _httpClient.DeleteAsync($"/api/chat/sessions/{nonExistentId}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
