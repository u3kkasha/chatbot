using Chatbot.Modules.Identity.Models.Users;
using NodaTime;
using Shouldly;

namespace Chatbot.Tests.Unit.Models.Users;

public class UserTests
{
    [Fact]
    public void ShouldCreateUserRecord()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var tenantId = TenantId.From(Guid.NewGuid());
        var now = Instant.FromUnixTimeSeconds(1000);

        // Act
        var user = new User(
            Id: userId,
            TenantId: tenantId,
            Username: "testuser",
            Email: "test@example.com",
            PasswordHash: "hash",
            Role: "Agent",
            CreatedDate: now,
            UpdatedDate: now
        );

        // Assert
        user.Id.ShouldBe(userId);
        user.TenantId.ShouldBe(tenantId);
        user.Username.ShouldBe("testuser");
        user.Role.ShouldBe("Agent");
    }
}

