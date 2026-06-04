using Chatbot.Modules.Identity.Brokers.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Modules.Identity;

public static class ModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddDbContext<IStorageBroker, StorageBroker>();

        return services;
    }
}
