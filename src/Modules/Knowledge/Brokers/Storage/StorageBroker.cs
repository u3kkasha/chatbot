using System.Diagnostics.CodeAnalysis;
using Chatbot.Modules.Knowledge.Brokers.Storage.CompiledModels;
using Chatbot.Modules.Knowledge.Models.Documents;
using Chatbot.Shared.Infrastructure.Data;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Knowledge.Brokers.Storage;

[RequiresDynamicCode("EF Core storage brokers use compiled models for AOT compatibility.")]
[RequiresUnreferencedCode("EF Core storage brokers use compiled models for AOT compatibility.")]
public partial class StorageBroker(DbContextOptions<StorageBroker> options)
    : DbContext(options),
        IStorageBroker
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("knowledge");

        modelBuilder.Entity<KnowledgeDocument>(document =>
        {
            document
                .Property(d => d.Id)
                .HasConversion(id => id.Value, value => new KnowledgeDocumentId(value));
            document
                .Property(d => d.TenantId)
                .HasConversion(id => id.Value, value => new TenantId(value));
        });

        modelBuilder.Entity<DocumentChunk>(chunk =>
        {
            chunk
                .Property(c => c.Id)
                .HasConversion(id => id.Value, value => new DocumentChunkId(value));
            chunk
                .Property(c => c.DocumentId)
                .HasConversion(id => id.Value, value => new KnowledgeDocumentId(value));
            chunk
                .Property(c => c.TenantId)
                .HasConversion(id => id.Value, value => new TenantId(value));
        });
    }
}
