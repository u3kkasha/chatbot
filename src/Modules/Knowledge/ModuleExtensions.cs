using Chatbot.Modules.Knowledge.Brokers.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Modules.Knowledge;

public static class ModuleExtensions
{
    public static IServiceCollection AddKnowledgeModule(this IServiceCollection services)
    {
        services.AddDbContext<IStorageBroker, StorageBroker>();
        return services;
    }
}
