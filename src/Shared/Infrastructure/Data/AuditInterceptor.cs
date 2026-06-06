using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;

namespace Chatbot.Shared.Infrastructure.Data;

public class AuditInterceptor(IClock clock) : SaveChangesInterceptor
{
    private readonly IClock clock = clock;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var now = this.clock.GetCurrentInstant();

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            var createdDateProp = entry.Metadata.FindProperty("CreatedDate");
            var updatedDateProp = entry.Metadata.FindProperty("UpdatedDate");

            if (entry.State == EntityState.Added)
            {
                if (createdDateProp != null && createdDateProp.ClrType == typeof(Instant))
                {
                    entry.Property("CreatedDate").CurrentValue = now;
                }
                if (updatedDateProp != null && updatedDateProp.ClrType == typeof(Instant))
                {
                    entry.Property("UpdatedDate").CurrentValue = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (updatedDateProp != null && updatedDateProp.ClrType == typeof(Instant))
                {
                    entry.Property("UpdatedDate").CurrentValue = now;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
