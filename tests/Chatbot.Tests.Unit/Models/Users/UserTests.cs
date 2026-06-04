using Chatbot.Modules.Identity.Models.Users;
using FluentAssertions;
using NodaTime;

namespace Chatbot.Tests.Unit.Models.Users;

public class UserTests
{
    [Fact]
    public void ShouldCreateUserRecord()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var now = Instant.FromUnixTimeSeconds(1000);

        // Act
        var user = new User(
            Id: userId,
            Username: "testuser",
            Email: "test@example.com",
            PasswordHash: "hash",
            CreatedDate: now,
            UpdatedDate: now
        );

        // Assert
        user.Id.Should().Be(userId);
        user.Username.Should().Be("testuser");
    }
}
