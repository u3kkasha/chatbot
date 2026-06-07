using System.Diagnostics.CodeAnalysis;
using Chatbot.Modules.Identity.Brokers.Storage.CompiledModels;
using Chatbot.Modules.Identity.Models.Users;
using Chatbot.Shared.Models;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Identity.Brokers.Storage;

[SuppressMessage("Trimming", "IL2026", Justification = "EF Core storage brokers use compiled models for AOT compatibility.")]
[SuppressMessage("AOT", "IL3050", Justification = "EF Core storage brokers use compiled models for AOT compatibility.")]
public partial class StorageBroker(DbContextOptions<StorageBroker> options)
    : DbContext(options),
        IStorageBroker
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Models.Users.User>(user =>
        {
            user.Property(u => u.Id).HasConversion(id => id.Value, value => new UserId(value));
            user.Property(u => u.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        });
    }
}
