using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.AiUsage;
using Chatbot.Shared.Brokers.Blobs;
using Chatbot.Shared.Brokers.DateTimes;
using Chatbot.Shared.Brokers.DistributedLock;
using Chatbot.Shared.Brokers.Events;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Brokers.Pii;
using Chatbot.Shared.Brokers.Processing;
using Chatbot.Shared.Brokers.Vectors;
using Microsoft.Extensions.VectorData;

using Qdrant.Client;
using StackExchange.Redis;
using Chatbot.Shared.Infrastructure.Configuration;
using Chatbot.Shared.Infrastructure.Data;
using Chatbot.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using NodaTime;

namespace Chatbot.Shared;

public static class SharedExtensions
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Options Binding
        services.Configure<ConnectionStringsOptions>(configuration.GetSection(ConnectionStringsOptions.SectionName));
        services.Configure<QdrantOptions>(configuration.GetSection(QdrantOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<ProcessingOptions>(configuration.GetSection(ProcessingOptions.SectionName));

        // NodaTime
        services.AddSingleton<IClock>(SystemClock.Instance);

        // Resilience
        services.AddStandardResilience();

        // Caching & Redis Connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionStrings = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
            if (string.IsNullOrEmpty(connectionStrings.Redis))
            {
                throw new InvalidOperationException("Redis connection string not configured.");
            }
            return ConnectionMultiplexer.Connect(connectionStrings.Redis);
        });

        services.AddStackExchangeRedisCache(options => { });
        services.AddTransient<IConfigureOptions<RedisCacheOptions>, ConfigureRedisCacheOptions>();

        // Typed Http Clients
        services.AddHttpClient<IOpenRouterClient, OpenRouterClient>((sp, client) =>
        {
            var aiOptions = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            client.BaseAddress = new Uri(aiOptions.Endpoint.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiOptions.ApiKey}");
        });

        services.AddHttpClient<IDoclingClient, DoclingClient>((sp, client) =>
        {
            var processingOptions = sp.GetRequiredService<IOptions<ProcessingOptions>>().Value;
            client.BaseAddress = new Uri(processingOptions.BaseUrl);
        });

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this since we are in net10.0 and this is a core feature we want to use.
        services.AddHybridCache();
#pragma warning restore EXTEXP0018

        // Data Infrastructure
        services.AddSingleton<AuditInterceptor>();
        services.AddSingleton<RlsInterceptor>();

        // Native Qdrant Client
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return new QdrantClient(opts.Host, opts.Port, opts.UseHttps, opts.ApiKey);
        });

        // Vector Store (Microsoft.Extensions.VectorData + Qdrant)
        // Registers QdrantClient + QdrantVectorStore as VectorStore via extension method.
        // IL2026/IL3050: MEVD dynamic mapping path requires reflection.
        // Suppressed: project is AOT-ready by design, not compiled with PublishAot=true.
#pragma warning disable IL2026, IL3050
        services.AddQdrantVectorStore();
#pragma warning restore IL2026, IL3050

        // Brokers
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<IPiiBroker, PiiBroker>();
        services.AddTransient<IAiUsageBroker, AiUsageBroker>();
        services.AddTransient<IBlobBroker, BlobBroker>();
        services.AddTransient<IVectorBroker, VectorBroker>();
        services.AddTransient<IQdrantBroker, QdrantBroker>();
        services.AddTransient<IAiBroker, AiBroker>();
        services.AddTransient<IProcessingBroker, ProcessingBroker>();
        services.AddTransient<IEventBroker, EventBroker>();
        services.AddTransient<IDistributedLockBroker, DistributedLockBroker>();

        return services;
    }
}

public class ConfigureRedisCacheOptions(IOptions<ConnectionStringsOptions> connectionStrings)
    : IConfigureOptions<RedisCacheOptions>
{
    public void Configure(RedisCacheOptions options)
    {
        options.Configuration = connectionStrings.Value.Redis ?? "localhost";
    }
}
