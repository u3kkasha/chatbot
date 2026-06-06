using Chatbot.Api.Infrastructure.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Api.Infrastructure.ExceptionHandlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var isBadRequest = exception is BadHttpRequestException;
        var statusCode = isBadRequest
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        if (isBadRequest)
        {
            logger.LogWarning(exception, "A bad request occurred: {Message}", exception.Message);
        }
        else
        {
            logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = isBadRequest ? "Bad Request" : "Internal Server Error",
            Type = isBadRequest
                ? "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
                : "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Detail = isBadRequest ? exception.Message : "An unexpected error occurred on the server.",
        };

        if (!isBadRequest && (httpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false))
        {
            problemDetails.Detail = exception.ToString();
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            AppJsonSerializerContext.Default.ProblemDetails,
            cancellationToken: cancellationToken
        );

        return true;
    }
}
