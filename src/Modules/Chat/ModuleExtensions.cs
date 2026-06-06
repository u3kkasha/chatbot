using Chatbot.Modules.Chat.Brokers.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Modules.Chat;

public static class ModuleExtensions
{
    public static IServiceCollection AddChatModule(this IServiceCollection services)
    {
        services.AddDbContext<IStorageBroker, StorageBroker>();
        return services;
    }
}
