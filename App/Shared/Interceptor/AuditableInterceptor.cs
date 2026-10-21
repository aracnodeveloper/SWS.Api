using App.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace App.Shared.Interceptor;

public class AuditableInterceptor : SaveChangesInterceptor
{
    private static void Apply(DbContextEventData eventData)
    {
        if (eventData.Context is null)
            return;

        var sessionAccesor = eventData.Context.GetService<SessionAccesor>();
        if (!sessionAccesor.IsAuditableEnabled)
            return;

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry is { State: EntityState.Modified, Entity: IAuditable modified })
            {
                modified.UpdatedAt = DateTime.UtcNow;
                modified.UpdatedBy = sessionAccesor.UserId;
            }
            else if (entry is { State: EntityState.Added, Entity: IAuditable added })
            {
                added.CreatedAt = DateTime.UtcNow;
                added.CreatedBy = sessionAccesor.UserId;
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        Apply(eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Apply(eventData);
        return new ValueTask<InterceptionResult<int>>(result);
    }
}
