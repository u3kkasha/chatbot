using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Chatbot.Tests.Integration.Brokers.Storage;

public class TestDatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgreSqlContainer { get; } =
        new PostgreSqlBuilder("postgres:18-alpine").Build();

    public RedisContainer RedisContainer { get; } =
        new RedisBuilder("ghcr.io/microsoft/garnet:latest")
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilMessageIsLogged("Ready to accept connections")
            )
            .Build();

    public IConfiguration Configuration { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(PostgreSqlContainer.StartAsync(), RedisContainer.StartAsync());

        var configurationSettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", PostgreSqlContainer.GetConnectionString() },
            { "ConnectionStrings:Redis", RedisContainer.GetConnectionString() },
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationSettings)
            .Build();
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(PostgreSqlContainer.StopAsync(), RedisContainer.StopAsync());
    }
}
