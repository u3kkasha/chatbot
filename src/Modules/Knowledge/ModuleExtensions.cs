using Chatbot.Modules.Knowledge.Brokers.Storage;
using Chatbot.Modules.Knowledge.Brokers.Storage.CompiledModels;
using Chatbot.Shared.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatbot.Modules.Knowledge;

public static class ModuleExtensions
{
    public static IServiceCollection AddKnowledgeModule(this IServiceCollection services)
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
        return services;
    }
}
