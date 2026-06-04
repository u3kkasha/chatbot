using Chatbot.Modules.Identity.Brokers.Storage;
using Chatbot.Modules.Identity.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Chatbot.Tests.Integration.Brokers.Storage;

public class StorageBrokerTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly StorageBroker _storageBroker = new(fixture.Configuration);

    [Fact]
    public async Task InsertUserAsync_ShouldInsertUser()
    {
        // Arrange
        await ((DbContext)_storageBroker).Database.EnsureCreatedAsync();

        var now = SystemClock.Instance.GetCurrentInstant();
        var user = new User(
            Id: UserId.From(Guid.NewGuid()),
            Username: "testuser",
            Email: "test@example.com",
            PasswordHash: "hash",
            CreatedDate: now,
            UpdatedDate: now
        );

        // Act
        var insertedUser = await _storageBroker.InsertUserAsync(user);

        // Assert
        insertedUser.Should().NotBeNull();
        insertedUser.Id.Should().Be(user.Id);

        var selectUser = await _storageBroker.SelectUserByIdAsync(user.Id);
        selectUser.Should().NotBeNull();
        selectUser!.Username.Should().Be(user.Username);
    }
}
