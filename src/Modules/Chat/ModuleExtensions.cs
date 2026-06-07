using System.Diagnostics.CodeAnalysis;
using Chatbot.Modules.Chat.Brokers.Storage;
using Chatbot.Modules.Chat.Brokers.Storage.CompiledModels;
using Chatbot.Modules.Chat.Features.Messages;
using Chatbot.Modules.Chat.Features.Sessions;
using Chatbot.Modules.Chat.Features.Sessions.Jobs;
using Chatbot.Modules.Chat.Features.StreamCompletion;
using Chatbot.Shared.Infrastructure.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Modules.Chat;

public static class ModuleExtensions
{
    public static IServiceCollection AddChatModule(this IServiceCollection services)
    {
        services.AddDbContextPool<IStorageBroker, StorageBroker>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
            var rlsInterceptor = sp.GetRequiredService<RlsInterceptor>();

            string connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new System.InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                );

            options
                .UseNpgsql(connectionString, x => x.UseNodaTime())
                .UseModel(StorageBrokerModel.Instance)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(auditInterceptor, rlsInterceptor);
        });

        services.AddScoped<IStreamCompletionService, StreamCompletionService>();
        services.AddScoped<IChatSessionService, ChatSessionService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();
        services.AddTransient<ChatSessionCleanupJob>();

        return services;
    }

    public static IEndpointRouteBuilder MapChatModule(this IEndpointRouteBuilder routes)
    {
        routes.MapStreamCompletion();
        routes.MapChatSessionEndpoints();
        routes.MapChatMessageEndpoints();

        return routes;
    }
}
