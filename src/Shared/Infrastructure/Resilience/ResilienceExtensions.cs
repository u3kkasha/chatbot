using System;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace Chatbot.Shared.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public static IServiceCollection AddStandardResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(
            "default",
            builder =>
            {
                builder.AddRetry(
                    new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromSeconds(1),
                    }
                );
            }
        );

        return services;
    }
}
