using System.Diagnostics.CodeAnalysis;
using Chatbot.Modules.Chat.Brokers.Storage.CompiledModels;
using Chatbot.Modules.Chat.Models.Sessions;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Chat.Brokers.Storage;

[SuppressMessage("Trimming", "IL2026", Justification = "EF Core storage brokers use compiled models for AOT compatibility.")]
[SuppressMessage("AOT", "IL3050", Justification = "EF Core storage brokers use compiled models for AOT compatibility.")]
public partial class StorageBroker(DbContextOptions<StorageBroker> options)
    : DbContext(options),
        IStorageBroker
{
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
