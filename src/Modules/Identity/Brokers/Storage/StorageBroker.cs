using Chatbot.Modules.Identity.Models.Users;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Identity.Brokers.Storage;

public partial class StorageBroker(IConfiguration configuration, AuditInterceptor auditInterceptor)
    : DbContext,
        IStorageBroker
{
    private readonly IConfiguration configuration = configuration;
    private readonly AuditInterceptor auditInterceptor = auditInterceptor;

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
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .AddInterceptors(this.auditInterceptor);
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

