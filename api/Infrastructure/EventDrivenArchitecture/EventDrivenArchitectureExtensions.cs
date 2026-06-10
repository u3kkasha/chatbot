using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Identity.Brokers.Storage;
using Chatbot.Modules.Knowledge.Brokers.Storage;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Api.Infrastructure.EventDrivenArchitecture;

public static class EventDrivenArchitectureExtensions
{
    public static IServiceCollection AddEventDrivenArchitecture(this IServiceCollection services, IConfiguration configuration)
    {
        var outboxProvider = configuration.GetValue<string>("MassTransit:Outbox:Provider") ?? "Postgres";

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            if (outboxProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                // Configure Transactional Outbox per StorageBroker
                x.AddEntityFrameworkOutbox<Modules.Chat.Brokers.Storage.StorageBroker>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.AddEntityFrameworkOutbox<Modules.Identity.Brokers.Storage.StorageBroker>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.AddEntityFrameworkOutbox<Modules.Knowledge.Brokers.Storage.StorageBroker>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });
            }

            // Automatically scan and register consumers from modules
            x.AddConsumers(typeof(Modules.Chat.ModuleExtensions).Assembly);
            x.AddConsumers(typeof(Modules.Identity.ModuleExtensions).Assembly);
            x.AddConsumers(typeof(Modules.Knowledge.ModuleExtensions).Assembly);

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
