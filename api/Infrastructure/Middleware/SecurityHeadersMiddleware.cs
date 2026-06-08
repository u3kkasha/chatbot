namespace Chatbot.Api.Infrastructure.Middleware;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            return Task.CompletedTask;
        });

        await next(context);
    }
}
