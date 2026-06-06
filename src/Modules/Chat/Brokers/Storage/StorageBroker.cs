using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Chat.Brokers.Storage;

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
            .AddInterceptors(this.auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("chat");

        modelBuilder.Entity<ChatSession>(session =>
        {
            session
                .Property(s => s.Id)
                .HasConversion(id => id.Value, value => ChatSessionId.From(value));
            session
                .Property(s => s.CustomerId)
                .HasConversion(id => id.Value, value => CustomerId.From(value));

            session
                .Property(s => s.OperatorId)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : default(Guid?),
                    value => value.HasValue ? OperatorId.From(value.Value) : default(OperatorId?)
                );
        });

        modelBuilder.Entity<ChatMessage>(message =>
        {
            message
                .Property(m => m.Id)
                .HasConversion(id => id.Value, value => ChatMessageId.From(value));
            message
                .Property(m => m.SessionId)
                .HasConversion(id => id.Value, value => ChatSessionId.From(value));

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
