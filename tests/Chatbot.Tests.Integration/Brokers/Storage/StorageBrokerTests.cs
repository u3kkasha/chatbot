using Chatbot.Modules.Identity.Brokers.Storage;
using Chatbot.Modules.Identity.Models.Users;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shouldly;

namespace Chatbot.Tests.Integration.Brokers.Storage;

public class StorageBrokerTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    private readonly StorageBroker _storageBroker = new(
        fixture.Configuration,
        new Chatbot.Shared.Infrastructure.Data.AuditInterceptor(SystemClock.Instance)
    );

    [Fact]
    public void StorageBroker_ShouldUseIdentitySchema()
    {
        // Arrange & Act
        var entityType = _storageBroker.Model.FindEntityType(typeof(User));
        var schema = entityType?.GetSchema();

        // Assert
        schema.ShouldBe("identity");
    }

    [Fact]
    public void StorageBroker_ShouldUseSnakeCaseNamingConvention()
    {
        // Arrange
        var entityType = _storageBroker.Model.FindEntityType(typeof(User));
        entityType.ShouldNotBeNull();

        // Act
        var tableName = entityType!.GetTableName();
        var idProperty = entityType.FindProperty(nameof(User.Id));
        var createdDateProperty = entityType.FindProperty(nameof(User.CreatedDate));

        var storeObject = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(
            tableName!,
            entityType.GetSchema()
        );
        var idColumnName = idProperty?.GetColumnName(storeObject);
        var createdDateColumnName = createdDateProperty?.GetColumnName(storeObject);

        // Assert
        tableName.ShouldBe("users");
        idColumnName.ShouldBe("id");
        createdDateColumnName.ShouldBe("created_date");
    }

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
        insertedUser.ShouldNotBeNull();
        insertedUser.Id.ShouldBe(user.Id);

        var selectUser = await _storageBroker.SelectUserByIdAsync(user.Id);
        selectUser.ShouldNotBeNull();
        selectUser!.Username.ShouldBe(user.Username);
    }
}
