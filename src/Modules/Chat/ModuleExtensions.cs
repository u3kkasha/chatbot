using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Features.Messages;
using Chatbot.Modules.Chat.Features.Sessions;
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
        services.AddScoped<IChatSessionService, ChatSessionService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();

        return services;
    }

    public static IEndpointRouteBuilder MapChatModule(this IEndpointRouteBuilder routes)
    {
        routes.MapStreamCompletion();

        return routes;
    }
}
