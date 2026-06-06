using Chatbot.Api.Infrastructure.Routing;
using FluentAssertions;

namespace Chatbot.Tests.Unit.Infrastructure.Routing;

public class KebabCaseRouteParameterTransformerTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("Identity", "identity")]
    [InlineData("UserSessions", "user-sessions")]
    [InlineData("OmnichannelSupportOperator", "omnichannel-support-operator")]
    [InlineData("V1Controller", "v1-controller")]
    public void TransformOutbound_ShouldConvertToKebabCase(string? input, string? expected)
    {
        // Arrange
        var transformer = new KebabCaseRouteParameterTransformer();

        // Act
        var result = transformer.TransformOutbound(input);

        // Assert
        result.Should().Be(expected);
    }
}
