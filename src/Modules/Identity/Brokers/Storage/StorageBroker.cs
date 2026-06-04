using Chatbot.Modules.Identity.Models.Users;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chatbot.Modules.Identity.Brokers.Storage;

public partial class StorageBroker(IConfiguration configuration) : DbContext, IStorageBroker
{
    private readonly IConfiguration configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new System.InvalidOperationException(
                "Connection string 'DefaultConnection' not found."
            );

        optionsBuilder
            .UseNpgsql(connectionString, x => x.UseNodaTime())
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vogen type conversions would go here if not handled globally
        modelBuilder.Entity<Models.Users.User>(user =>
        {
            user.Property(u => u.Id).HasConversion(id => id.Value, value => UserId.From(value));
        });
    }
}
