using Chatbot.Api.Infrastructure.ExceptionHandlers;
using Chatbot.Api.Infrastructure.Middleware;
using Chatbot.Api.Infrastructure.Routing;
using Chatbot.Api.Infrastructure.Serialization;
using Chatbot.Modules.Chat;
using Chatbot.Modules.Identity;
using Chatbot.Modules.Knowledge;
using Chatbot.Shared;
using Chatbot.Shared.Brokers.Ai;
using Coravel;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.AI;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add Infrastructure Services
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSharedInfrastructure(builder.Configuration);

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

// 2.3 Add AI Client
builder.Services.AddSingleton<IChatClient, NoopChatClient>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new KebabCaseRouteParameterTransformer())
    );
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
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new HealthCheckResponse("Healthy", DateTime.UtcNow)));

app.Run();

public partial class Program { }
