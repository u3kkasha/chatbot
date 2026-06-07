using Chatbot.Modules.Chat.Brokers.Storage.CompiledModels;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Chat.Brokers.Storage;

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
            .UseModel(StorageBrokerModel.Instance)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(this.auditInterceptor, this.rlsInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("chat");

        modelBuilder.Entity<ChatSession>(session =>
        {
            session
                .Property(s => s.Id)
                .HasConversion(id => id.Value, value => new ChatSessionId(value));
            session
                .Property(s => s.TenantId)
                .HasConversion(id => id.Value, value => new TenantId(value));
            session
                .Property(s => s.OperatorId)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : default(Guid?),
                    value => value.HasValue ? new OperatorId(value.Value) : default(OperatorId?)
                );
            session
                .Property(s => s.ChannelProvider)
                .HasConversion<string>();

            session.HasIndex(s => new { s.ChannelProvider, s.ExternalReferenceId })
                .HasDatabaseName("idx_sessions_channel");
        });

        modelBuilder.Entity<ChatMessage>(message =>
        {
            message
                .Property(m => m.Id)
                .HasConversion(id => id.Value, value => new ChatMessageId(value));
            message
                .Property(m => m.SessionId)
                .HasConversion(id => id.Value, value => new ChatSessionId(value));
            message
                .Property(m => m.TenantId)
                .HasConversion(id => id.Value, value => new TenantId(value));
            message
                .Property(m => m.Status)
                .HasConversion<string>();

            message.HasIndex(m => m.SessionId)
                .HasDatabaseName("idx_messages_session");

            // Native JSON Mapping for citations and AI metadata
            message.OwnsMany(
                m => m.Citations,
                c =>
                {
                    c.ToJson();
                }
            );
            message.OwnsOne(
                m => m.AiMetadata,
                am =>
                {
                    am.ToJson();
                }
            );
        });
    }
}
