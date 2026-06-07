using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Chatbot.Modules.Chat.Features.Messages;
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

public class ChatMessageEndpointsTests
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

    public ChatMessageEndpointsTests(TestDatabaseFixture fixture, IntegrationTestWebApplicationFactory factory)
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

    private async Task<(Guid SessionId, Guid TenantId)> CreateTestSessionAsync()
    {
        var tenantId = Guid.NewGuid();
        var request = new CreateChatSessionRequest(
            TenantId: tenantId,
            ChannelProvider: "WebWidget",
            ExternalReferenceId: null,
            CustomerIdentifier: "customer@example.com",
            OperatorId: null
        );
        var response = await _httpClient.PostAsJsonAsync("/api/chat/sessions", request, JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<ChatSessionResponse>(JsonOptions);
        session.ShouldNotBeNull();
        return (session.Id, tenantId);
    }

    private static CreateChatMessageRequest MakeCreateRequest(
        Guid sessionId,
        Guid tenantId,
        string sender = "Customer",
        string content = "Hello operator!",
        string status = "Sent",
        bool isAiGenerated = false
    ) => new(
        SessionId: sessionId,
        TenantId: tenantId,
        Sender: sender,
        Content: content,
        Status: status,
        IsAiGenerated: isAiGenerated,
        ApprovedBy: null,
        Citations: null,
        AiMetadata: null
    );

    [Fact]
    public async Task PostChatMessage_ReturnsCreatedAndMessage()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var (sessionId, tenantId) = await CreateTestSessionAsync();
        var request = MakeCreateRequest(sessionId, tenantId, content: "Hello operator!");

        // when
        var response = await _httpClient.PostAsJsonAsync("/api/chat/messages", request, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var message = await response.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        message.ShouldNotBeNull();
        message.Id.ShouldNotBe(Guid.Empty);
        message.SessionId.ShouldBe(sessionId);
        message.TenantId.ShouldBe(tenantId);
        message.Sender.ShouldBe(MessageSender.Customer.ToString());
        message.Content.ShouldBe("Hello operator!");
        message.Status.ShouldBe(MessageStatus.Sent.ToString());
        message.IsAiGenerated.ShouldBeFalse();
    }

    [Fact]
    public async Task PostChatMessage_ValidationFailure_ReturnsBadRequest()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var request = new CreateChatMessageRequest(
            SessionId: Guid.Empty,
            TenantId: Guid.NewGuid(),
            Sender: "InvalidSender",
            Content: "",
            Status: "Sent",
            IsAiGenerated: false,
            ApprovedBy: null,
            Citations: null,
            AiMetadata: null
        );

        // when
        var response = await _httpClient.PostAsJsonAsync("/api/chat/messages", request, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(JsonOptions);
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(400);
        problem.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetChatMessageById_ReturnsOk()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var (sessionId, tenantId) = await CreateTestSessionAsync();
        var request = MakeCreateRequest(sessionId, tenantId, content: "Test message");
        var createResponse = await _httpClient.PostAsJsonAsync("/api/chat/messages", request, JsonOptions);
        var createdMessage = await createResponse.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        createdMessage.ShouldNotBeNull();

        // when
        var response = await _httpClient.GetAsync($"/api/chat/messages/{createdMessage.Id}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        message.ShouldNotBeNull();
        message.Id.ShouldBe(createdMessage.Id);
    }

    [Fact]
    public async Task GetChatMessageById_NotFound_ReturnsNotFound()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var nonExistentId = Guid.NewGuid();

        // when
        var response = await _httpClient.GetAsync($"/api/chat/messages/{nonExistentId}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllChatMessages_ReturnsOk()
    {
        // given
        await EnsureDatabaseCreatedAsync();

        // when
        var response = await _httpClient.GetAsync("/api/chat/messages");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var messages = await response.Content.ReadFromJsonAsync<List<ChatMessageResponse>>(JsonOptions);
        messages.ShouldNotBeNull();
    }

    [Fact]
    public async Task PutChatMessage_ReturnsOkAndUpdatedMessage()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var (sessionId, tenantId) = await CreateTestSessionAsync();
        var request = MakeCreateRequest(sessionId, tenantId, content: "Original Content");
        var createResponse = await _httpClient.PostAsJsonAsync("/api/chat/messages", request, JsonOptions);
        var createdMessage = await createResponse.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        createdMessage.ShouldNotBeNull();

        var updateRequest = new UpdateChatMessageRequest(
            SessionId: sessionId,
            TenantId: tenantId,
            Sender: MessageSender.Customer.ToString(),
            Content: "Updated Content",
            Status: MessageStatus.Delivered.ToString(),
            IsAiGenerated: false,
            ApprovedBy: null,
            Citations: null,
            AiMetadata: null
        );

        // when
        var response = await _httpClient.PutAsJsonAsync($"/api/chat/messages/{createdMessage.Id}", updateRequest, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedMessage = await response.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        updatedMessage.ShouldNotBeNull();
        updatedMessage.Content.ShouldBe("Updated Content");
        updatedMessage.Status.ShouldBe(MessageStatus.Delivered.ToString());
    }

    [Fact]
    public async Task PutChatMessage_NotFound_ReturnsNotFound()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateChatMessageRequest(
            SessionId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Sender: MessageSender.Customer.ToString(),
            Content: "Some Content",
            Status: MessageStatus.Sent.ToString(),
            IsAiGenerated: false,
            ApprovedBy: null,
            Citations: null,
            AiMetadata: null
        );

        // when
        var response = await _httpClient.PutAsJsonAsync($"/api/chat/messages/{nonExistentId}", updateRequest, JsonOptions);

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteChatMessage_ReturnsOkAndDeletedMessage()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var (sessionId, tenantId) = await CreateTestSessionAsync();
        var request = MakeCreateRequest(sessionId, tenantId, content: "Delete Me");
        var createResponse = await _httpClient.PostAsJsonAsync("/api/chat/messages", request, JsonOptions);
        var createdMessage = await createResponse.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        createdMessage.ShouldNotBeNull();

        // when
        var response = await _httpClient.DeleteAsync($"/api/chat/messages/{createdMessage.Id}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deletedMessage = await response.Content.ReadFromJsonAsync<ChatMessageResponse>(JsonOptions);
        deletedMessage.ShouldNotBeNull();
        deletedMessage.Id.ShouldBe(createdMessage.Id);
    }

    [Fact]
    public async Task DeleteChatMessage_NotFound_ReturnsNotFound()
    {
        // given
        await EnsureDatabaseCreatedAsync();
        var nonExistentId = Guid.NewGuid();

        // when
        var response = await _httpClient.DeleteAsync($"/api/chat/messages/{nonExistentId}");

        // then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
