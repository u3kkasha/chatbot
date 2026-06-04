using Chatbot.Api.Infrastructure.ExceptionHandlers;
using Chatbot.Api.Infrastructure.Middleware;
using Chatbot.Modules.Chat;
using Chatbot.Modules.Identity;
using Chatbot.Modules.Knowledge;
using Chatbot.Shared;
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
builder.Services.AddControllers();

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

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();

public partial class Program { }
