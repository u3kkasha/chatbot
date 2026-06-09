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
using Microsoft.SemanticKernel.Connectors.Qdrant;
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
        // Options & Secrets Validation
        bool isGeneratingOpenApi = IsGeneratingOpenApi();

        var connectionStringsBuilder = services
            .AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName));
        if (!isGeneratingOpenApi)
        {
            connectionStringsBuilder.ValidateOnStart();
        }
        services.AddSingleton<
            IValidateOptions<ConnectionStringsOptions>,
            ConnectionStringsOptionsValidator
        >();

        var qdrantBuilder = services
            .AddOptions<QdrantOptions>()
            .Bind(configuration.GetSection(QdrantOptions.SectionName));
        if (!isGeneratingOpenApi)
        {
            qdrantBuilder.ValidateOnStart();
        }
        services.AddSingleton<IValidateOptions<QdrantOptions>, QdrantOptionsValidator>();

        var aiBuilder = services
            .AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName));
        if (!isGeneratingOpenApi)
        {
            aiBuilder.ValidateOnStart();
        }
        services.AddSingleton<IValidateOptions<AiOptions>, AiOptionsValidator>();

        var processingBuilder = services
            .AddOptions<ProcessingOptions>()
            .Bind(configuration.GetSection(ProcessingOptions.SectionName));
        if (!isGeneratingOpenApi)
        {
            processingBuilder.ValidateOnStart();
        }
        services.AddSingleton<IValidateOptions<ProcessingOptions>, ProcessingOptionsValidator>();

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

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this since we are in net10.0 and this is a core feature we want to use.
        services.AddHybridCache();
#pragma warning restore EXTEXP0018

        // Data Infrastructure
        services.AddSingleton<AuditInterceptor>();
        services.AddSingleton<RlsInterceptor>();

        // Vector Store (Microsoft.Extensions.VectorData + Qdrant)
        // Registers QdrantClient + QdrantVectorStore as VectorStore via extension method.
        // IL2026/IL3050: MEVD dynamic mapping path requires reflection.
        // Suppressed: project is AOT-ready by design, not compiled with PublishAot=true.
#pragma warning disable IL2026, IL3050
        services.AddQdrantVectorStore(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
            return new QdrantClient(opts.Host, opts.Port, opts.UseHttps, opts.ApiKey);
        });
#pragma warning restore IL2026, IL3050

        // Brokers
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<IPiiBroker, PiiBroker>();
        services.AddTransient<IAiUsageBroker, AiUsageBroker>();
        services.AddTransient<IBlobBroker, BlobBroker>();
        services.AddTransient<IVectorBroker, VectorBroker>();
        services.AddTransient<IAiBroker, AiBroker>();
        services.AddTransient<IProcessingBroker, ProcessingBroker>();
        services.AddTransient<IEventBroker, EventBroker>();
        services.AddTransient<IDistributedLockBroker, DistributedLockBroker>();

        return services;
    }

    private static bool IsGeneratingOpenApi()
    {
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        if (entryAssembly == null) return false;

        return entryAssembly.StartsWith("GetDocument", StringComparison.OrdinalIgnoreCase) ||
               entryAssembly.StartsWith("dotnet-getdocument", StringComparison.OrdinalIgnoreCase);
    }
}

public class ConfigureRedisCacheOptions(IOptions<ConnectionStringsOptions> connectionStrings)
    : IConfigureOptions<RedisCacheOptions>
{
    public void Configure(RedisCacheOptions options)
    {
        if (string.IsNullOrEmpty(connectionStrings.Value.Redis))
        {
            throw new InvalidOperationException("Redis connection string not configured.");
        }
        options.Configuration = connectionStrings.Value.Redis;
    }
}
