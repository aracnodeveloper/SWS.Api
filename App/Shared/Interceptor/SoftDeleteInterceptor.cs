using App.Modules.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace App.Shared.Interceptor;

/// <summary>
/// Convierte DELETE en UPDATE con <see cref="IEntityStatus.IsStatus"/> = false.
/// <see cref="UserEntity"/> se excluye (borrado físico).
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private static void Apply(DbContextEventData eventData)
    {
        if (eventData.Context is null)
            return;

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is UserEntity)
                continue;
            if (entry is not { State: EntityState.Deleted, Entity: IEntityStatus })
                continue;

            // Al pasar de Deleted → Modified, EF a veces no marca IsStatus como modificado
            // y el UPDATE no persiste el soft-delete (el registro sigue con IsStatus=true).
            entry.State = EntityState.Modified;
            entry.Property(nameof(IEntityStatus.IsStatus)).CurrentValue = false;
            entry.Property(nameof(IEntityStatus.IsStatus)).IsModified = true;
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
