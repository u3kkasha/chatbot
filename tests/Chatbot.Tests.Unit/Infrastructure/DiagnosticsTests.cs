using Chatbot.Api.Infrastructure.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shouldly;
using Xunit;

namespace Chatbot.Tests.Unit.Infrastructure;

public class DiagnosticsTests
{
    [Fact]
    public void AddDiagnostics_ShouldRegisterExpectedServices()
    {
        // given
        var services = new ServiceCollection();

        // We need to register Logger to satisfy dependencies if any, but standard Microsoft.Extensions.Logging is enough.
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:5341/ingest/otlp" }
            })
            .Build();

        // when
        services.AddDiagnostics(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // then
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();
        healthCheckService.ShouldNotBeNull();

        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        tracerProvider.ShouldNotBeNull();

        var meterProvider = serviceProvider.GetService<MeterProvider>();
        meterProvider.ShouldNotBeNull();
    }
}
