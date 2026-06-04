using Chatbot.Shared.Brokers.Ai;
using Chatbot.Shared.Brokers.AiUsage;
using Chatbot.Shared.Brokers.Blobs;
using Chatbot.Shared.Brokers.DateTimes;
using Chatbot.Shared.Brokers.Logging;
using Chatbot.Shared.Brokers.Pii;
using Chatbot.Shared.Brokers.Processing;
using Chatbot.Shared.Brokers.Vectors;
using Chatbot.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Chatbot.Shared;

public static class SharedExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        // NodaTime
        services.AddSingleton<IClock>(SystemClock.Instance);

        // Resilience
        services.AddStandardResilience();

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
