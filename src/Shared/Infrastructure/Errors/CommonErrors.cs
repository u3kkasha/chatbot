using System.Collections.Generic;

namespace Chatbot.Shared.Infrastructure.Errors;

public record ValidationError(string Message, Dictionary<string, List<string>>? Errors = null)
{
    public static ValidationError From(string propertyName, string errorMessage) =>
        new(errorMessage, new Dictionary<string, List<string>> { { propertyName, [errorMessage] } });
}

public record NotFoundError(string Message);
public record ConflictError(string Message);
public record UnauthorizedError(string Message);
