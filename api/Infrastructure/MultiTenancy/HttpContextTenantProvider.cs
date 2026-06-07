using Chatbot.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Chatbot.Api.Infrastructure.MultiTenancy;

public sealed class HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    private const string TenantIdHeader = "X-Tenant-Id";

    public Guid? GetTenantId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null)
        {
            return null;
        }

        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var tenantIdStr) &&
            Guid.TryParse(tenantIdStr, out var tenantIdGuid))
        {
            return tenantIdGuid;
        }

        return null;
    }
}
