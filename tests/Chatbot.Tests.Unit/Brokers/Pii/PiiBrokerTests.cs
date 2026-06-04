using Chatbot.Shared.Brokers.Pii;
using FluentAssertions;

namespace Chatbot.Tests.Unit.Brokers.Pii;

public class PiiBrokerTests
{
    private readonly IPiiBroker _piiBroker;

    public PiiBrokerTests()
    {
        _piiBroker = new PiiBroker();
    }

    [Theory]
    [InlineData("My email is test@example.com", "My email is [EMAIL]")]
    [InlineData("Call me at 555-123-4567", "Call me at [PHONE_NUMBER]")]
    [InlineData("My IP is 192.168.1.1", "My IP is [IP_ADDRESS]")]
    [InlineData(
        "Contact me at test@example.com or 555-123-4567",
        "Contact me at [EMAIL] or [PHONE_NUMBER]"
    )]
    public void MaskSensitiveData_ShouldMaskPii(string input, string expected)
    {
        // Act
        var result = _piiBroker.MaskSensitiveData(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void MaskSensitiveData_ShouldReturnOriginal_WhenNoPiiFound()
    {
        // Arrange
        var input = "Hello, how are you?";

        // Act
        var result = _piiBroker.MaskSensitiveData(input);

        // Assert
        result.Should().Be(input);
    }
}
