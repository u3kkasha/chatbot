using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace Chatbot.Api.Infrastructure.Routing;

public class KebabCaseRouteParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return Regex
            .Replace(
                value.ToString()!,
                "([a-z0-9])([A-Z])",
                "$1-$2",
                RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(100)
            )
            .ToLowerInvariant();
    }
}
