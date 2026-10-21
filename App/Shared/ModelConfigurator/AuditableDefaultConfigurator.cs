using App.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace App.Shared.ModelConfigurator;

internal static class AuditableDefaultConfigurator
{
    internal static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.EntitiesOfType<IAuditable>(builder =>
        {
            builder
                .Property<DateTime>(nameof(IAuditable.CreatedAt))
                .HasDefaultValueSql("NOW()");
        });
    }
}
