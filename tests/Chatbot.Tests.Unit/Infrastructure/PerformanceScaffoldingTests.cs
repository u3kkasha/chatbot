using System.Text.Json;
using Chatbot.Api.Infrastructure.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chatbot.Tests.Unit.Infrastructure;

public class PerformanceScaffoldingTests
{
    private readonly IServiceProvider _serviceProvider;

    public PerformanceScaffoldingTests()
    {
        var services = new ServiceCollection();

        // Setup minimal services needed for verification
        services.AddHybridCache();
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(
                0,
                AppJsonSerializerContext.Default
            );
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void HybridCache_ShouldBeResolvableFromDI()
    {
        var cache = _serviceProvider.GetService<HybridCache>();
        Assert.NotNull(cache);
    }

    [Fact]
    public void JsonSerializer_ShouldUseCustomContext()
    {
        var options = _serviceProvider
            .GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()
            .Value;

        // Verify AppJsonSerializerContext is in the resolver chain
        Assert.Contains(
            AppJsonSerializerContext.Default,
            options.SerializerOptions.TypeInfoResolverChain
        );
    }
}
