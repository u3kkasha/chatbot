using Chatbot.Modules.Identity.Models.Users;
using Chatbot.Shared.Models;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Identity.Brokers.Storage;

public partial class StorageBroker(
    IConfiguration configuration,
    AuditInterceptor auditInterceptor,
    RlsInterceptor rlsInterceptor)
    : DbContext,
        IStorageBroker
{
    private readonly IConfiguration configuration = configuration;
    private readonly AuditInterceptor auditInterceptor = auditInterceptor;
    private readonly RlsInterceptor rlsInterceptor = rlsInterceptor;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new System.InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        optionsBuilder
            .UseNpgsql(connectionString, x => x.UseNodaTime())
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(this.auditInterceptor, this.rlsInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<Models.Users.User>(user =>
        {
            user.Property(u => u.Id).HasConversion(id => id.Value, value => UserId.From(value));
            user.Property(u => u.TenantId).HasConversion(id => id.Value, value => TenantId.From(value));
        });
    }
}
