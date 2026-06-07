using System.Data.Common;
using Chatbot.Shared.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chatbot.Shared.Infrastructure.Data;

public sealed class RlsInterceptor(ITenantProvider tenantProvider) : DbCommandInterceptor
{
    private readonly ITenantProvider tenantProvider = tenantProvider;

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        this.SetTenantId(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        this.SetTenantId(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        this.SetTenantId(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        this.SetTenantId(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        this.SetTenantId(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        this.SetTenantId(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    internal void SetTenantId(DbCommand command)
    {
        var tenantId = this.tenantProvider.GetTenantId();

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return;
        }

        // We use SET LOCAL so that it's only scoped to the current transaction/connection lease.
        // This is safe even with connection pooling.
        command.CommandText = $"SET LOCAL app.current_tenant_id = '{tenantId.Value}'; " + command.CommandText;
    }
}
