using System;
using System.Threading.Tasks;
using Chatbot.Shared.Infrastructure.Resilience;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Xunit;

namespace Chatbot.Tests.Unit.Infrastructure;

public class ResilienceTests
{
    [Fact]
    public async Task DefaultResiliencePipeline_ShouldRetryOnFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStandardResilience();
        var serviceProvider = services.BuildServiceProvider();

        var pipelineProvider = serviceProvider.GetRequiredService<
            ResiliencePipelineProvider<string>
        >();
        var pipeline = pipelineProvider.GetPipeline("default");

        int callCount = 0;

        // Act
        var result = await pipeline.ExecuteAsync(async token =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("Temporary failure");
            }
            return await ValueTask.FromResult("success");
        });

        // Assert
        result.Should().Be("success");
        callCount.Should().Be(3); // Attempt 1 (fail), Attempt 2 (fail), Attempt 3 (success)
    }

    [Fact]
    public async Task DefaultResiliencePipeline_ShouldPropagateException_AfterMaxRetries()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStandardResilience();
        var serviceProvider = services.BuildServiceProvider();

        var pipelineProvider = serviceProvider.GetRequiredService<
            ResiliencePipelineProvider<string>
        >();
        var pipeline = pipelineProvider.GetPipeline("default");

        int callCount = 0;

        // Act
        var act = async () =>
        {
            await pipeline.ExecuteAsync<string>(async token =>
            {
                callCount++;
                throw new InvalidOperationException("Persistent failure");
            });
        };

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        callCount.Should().Be(4); // Attempt 1 (initial) + 3 retries = 4 attempts total
    }
}
