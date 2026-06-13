using Chatbot.Api.Infrastructure.Diagnostics;
using Chatbot.Api.Infrastructure.ExceptionHandlers;
using Chatbot.Api.Infrastructure.Middleware;
using Chatbot.Api.Infrastructure.MultiTenancy;
using Chatbot.Api.Infrastructure.Serialization;
using Chatbot.Modules.Chat;
using Chatbot.Modules.Chat.Features.Messages.Consumers;
using Chatbot.Modules.Chat.Features.Realtime;
using Chatbot.Modules.Chat.Features.Sessions.Jobs;
using Chatbot.Modules.Identity;
using Chatbot.Modules.Knowledge;
using Chatbot.Modules.Chat.Infrastructure;
using Chatbot.Shared;
using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Events;
using Chatbot.Shared.Models;
using Coravel;
using Chatbot.Shared.Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel options to disable Server header
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// Configure DI container validation rules
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});


// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add Infrastructure Services
builder.Services.AddOpenApi("openapi", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Omnichannel Chatbot API";
        document.Info.Version = "1.0.0";
        document.Info.Description = "API for the Omnichannel Customer Support Operator Platform.";
        return Task.CompletedTask;
    });
});
builder.Services.AddDiagnostics(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddSignalR();

// 2.1 Add Coravel
builder.Services.AddScheduler();
builder.Services.AddQueue();
builder.Services.AddEvents();

// 2.2 Add HybridCache
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5),
    };
});

// 2.3 Add AI Client (OpenRouter)
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
    return new OpenAI.OpenAIClient(new System.ClientModel.ApiKeyCredential(options.ApiKey), new OpenAI.OpenAIClientOptions
    {
        Endpoint = new Uri(options.Endpoint)
    });
});

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
    var client = sp.GetRequiredService<OpenAI.OpenAIClient>();

    return client.GetChatClient(options.ModelId).AsIChatClient();
});

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
    var client = sp.GetRequiredService<OpenAI.OpenAIClient>();

    return client.GetEmbeddingClient(options.EmbeddingModelId).AsIEmbeddingGenerator();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// 3. Add Domain Modules
builder.Services.AddIdentityModule().AddChatModule().AddKnowledgeModule();

// 4. Configure API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// 4. Configure Middleware Pipeline
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.MapGet("/identity", () => TypedResults.Ok(nameof(IdentityModule)));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Omnichannel Chatbot API"));
}

app.UseCors();

app.MapDiagnosticsEndpoints();

// 5. Map Module Endpoints
app.MapChatModule();
app.MapHub<ChatHub>("/hubs/chat");

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<ChatSessionCleanupJob>().EveryMinute();
});

app.Services.ConfigureEvents()
    .Register<ChatMessageAddedEvent>()
    .Subscribe<ChatMessageAddedListener>()
    .Subscribe<ChatMessageAddedRealtimeListener>();

app.Services.ConfigureEvents()
    .Register<ChatSessionStatusChangedEvent>()
    .Subscribe<ChatSessionStatusChangedRealtimeListener>();

if (args.Contains("--seed"))
{
    await ChatSeeder.SeedAsync(app.Services);
    return;
}

app.Run();

public partial class Program { }
