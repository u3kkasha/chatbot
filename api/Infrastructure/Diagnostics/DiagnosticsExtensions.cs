using Chatbot.Api.Infrastructure.Serialization;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Identity.Brokers.Storage;
using Chatbot.Modules.Knowledge.Brokers.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Chatbot.Api.Infrastructure.Diagnostics;

public static class DiagnosticsExtensions
{
    public static IServiceCollection AddDiagnostics(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 1. Configure Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<Modules.Chat.Brokers.Storage.StorageBroker>("ChatDatabase")
            .AddDbContextCheck<Modules.Identity.Brokers.Storage.StorageBroker>("IdentityDatabase")
            .AddDbContextCheck<Modules.Knowledge.Brokers.Storage.StorageBroker>("KnowledgeDatabase");

        // 2. Configure OpenTelemetry
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:5341/ingest/otlp";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Chatbot.Api"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }));

        return services;
    }

    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new HealthCheckResponse(
                    report.Status.ToString(),
                    DateTime.UtcNow
                );

                await context.Response.WriteAsJsonAsync(
                    response,
                    AppJsonSerializerContext.Default.HealthCheckResponse
                );
            }
        });

        return endpoints;
    }
}
