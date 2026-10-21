using Microsoft.EntityFrameworkCore;

namespace App.Shared;

/// <summary>
/// Filtro opcional por <see cref="IEntityStatus.IsStatus"/> frente al filtro global (solo activos).
/// </summary>
public static class EntityStatusQueryableExtensions
{
    public static IQueryable<TEntity> AsQueryableIncludingInactive<TEntity>(
        this DbSet<TEntity> source
    )
        where TEntity : class, IEntityStatus
    {
        return source.IgnoreQueryFilters();
    }

    public static TEntity? FindIncludingInactive<TEntity>(this DbSet<TEntity> set, Guid id)
        where TEntity : class, IEntity, IEntityStatus
    {
        return set.IgnoreQueryFilters().FirstOrDefault(e => e.Id == id);
    }

    /// <param name="isStatus">
    /// null o true: solo activos.
    /// false: solo inactivos / soft-deleted.
    /// Para listar todos, usar <see cref="AsQueryableIncludingInactive{TEntity}"/>.
    /// </param>
    public static IQueryable<TEntity> FilterByOptionalIsStatus<TEntity>(
        this IQueryable<TEntity> source,
        bool? isStatus
    )
        where TEntity : class, IEntityStatus
    {
        // Siempre IgnoreQueryFilters + predicado explícito: al proyectar navegaciones
        // (Offer, User, Establishment…) EF puede “perder” el query filter global.
        if (isStatus is null || isStatus.Value)
            return source.IgnoreQueryFilters().Where(e => e.IsStatus);

        return source.IgnoreQueryFilters().Where(e => !e.IsStatus);
    }
}
