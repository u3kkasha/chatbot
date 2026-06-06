using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.AiUsage;
using Chatbot.Shared.Brokers.Blobs;
using Chatbot.Shared.Brokers.DateTimes;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Brokers.Pii;
using Chatbot.Shared.Brokers.Processing;
using Chatbot.Shared.Brokers.Vectors;
using Chatbot.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Chatbot.Shared;

public static class SharedExtensions
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // NodaTime
        services.AddSingleton<IClock>(SystemClock.Instance);

        // Resilience
        services.AddStandardResilience();

        // Caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this since we are in net10.0 and this is a core feature we want to use.
        services.AddHybridCache();
#pragma warning restore EXTEXP0018

        // Data Infrastructure
        services.AddSingleton<Chatbot.Shared.Infrastructure.Data.AuditInterceptor>();

        // Brokers
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<IPiiBroker, PiiBroker>();
        services.AddTransient<IAiUsageBroker, AiUsageBroker>();
        services.AddTransient<IBlobBroker, BlobBroker>();
        services.AddTransient<IVectorBroker, VectorBroker>();
        services.AddTransient<IAiBroker, AiBroker>();
        services.AddTransient<IProcessingBroker, ProcessingBroker>();

        return services;
    }
}
