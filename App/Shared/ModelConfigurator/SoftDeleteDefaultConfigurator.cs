using System.Linq.Expressions;
using App.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace App.Shared.ModelConfigurator;

internal static class SoftDeleteDefaultConfigurator
{
    internal static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.EntitiesOfType<IEntityStatus>(builder =>
        {
            builder.Property<bool>(nameof(IEntityStatus.IsStatus)).HasDefaultValue(true);
            var param = Expression.Parameter(builder.Metadata.ClrType, "p");
            var body = Expression.Equal(
                Expression.Property(param, nameof(IEntityStatus.IsStatus)),
                Expression.Constant(true)
            );
            builder.HasQueryFilter(Expression.Lambda(body, param));
        });
    }
}
