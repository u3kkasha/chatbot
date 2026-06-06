using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Features.StreamCompletion;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Modules.Chat;

public static class ModuleExtensions
{
    public static IServiceCollection AddChatModule(this IServiceCollection services)
    {
        services.AddDbContext<IStorageBroker, StorageBroker>();
        services.AddScoped<IStreamCompletionService, StreamCompletionService>();

        return services;
    }

    public static IEndpointRouteBuilder MapChatModule(this IEndpointRouteBuilder routes)
    {
        routes.MapStreamCompletion();

        return routes;
    }
}
