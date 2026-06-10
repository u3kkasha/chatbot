using System;
using System.Linq;
using Xunit;

namespace Chatbot.Tests.Integration.Infrastructure;

/// <summary>
/// Custom Fact attribute that skips the test if any of the specified environment variables are missing.
/// </summary>
public class LiveFactAttribute : FactAttribute
{
    public LiveFactAttribute(params string[] envVarNames)
    {
        var missingVars = envVarNames
            .Where(name => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
            .ToList();

        if (missingVars.Count > 0)
        {
            Skip = $"Live test - missing required environment variables: {string.Join(", ", missingVars)}";
        }
    }
}
