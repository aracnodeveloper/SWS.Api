using App.Modules.Logs;
using App.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Order;

/// <summary>Cancela o expira un pedido y devuelve al inventario el stock que tenía reservado.</summary>
public class CancelOrder(ApplicationContext context, IAppLogger logger)
{
    /// <param name="order">Pedido con <see cref="OrderEntity.Items"/> cargados y rastreado por el contexto.</param>
    /// <exception cref="DbUpdateConcurrencyException">Otro proceso modificó el pedido a la vez (se revierte todo).</exception>
    public async Task ExecuteAsync(OrderEntity order, string finalStatus, string note, CancellationToken ct)
    {
        if (!OrderStatus.ReleasesStock(finalStatus))
            throw new ArgumentException("Estado final inválido.", nameof(finalStatus));

        var ownTx = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            await ReleaseStockAsync(order, ct);
            order.Status = finalStatus;
            order.ExpiresAt = null;
            order.AppendNote(note);
            await context.SaveChangesAsync(ct);
            if (ownTx is not null)
                await ownTx.CommitAsync(ct);
        }
        finally
        {
            if (ownTx is not null)
                await ownTx.DisposeAsync();
        }

        logger.Log($"[Order] {order.Code} → {finalStatus}: {note}", LogType.INFO);
    }

    internal async Task ReleaseStockAsync(OrderEntity order, CancellationToken ct)
    {
        if (order.StockReleased)
            return;

        // IgnoreQueryFilters: si el producto se desactivó mientras tanto, igual recupera su stock.
        foreach (var item in order.Items)
            await context.Product.IgnoreQueryFilters()
                .Where(p => p.Id == item.ProductId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity), ct);

        order.StockReleased = true;
    }

    /// <summary>
    /// Vuelve a reservar el stock de un pedido que ya lo había devuelto (pago aprobado
    /// después de expirar). Todo o nada: si falta alguna unidad no descuenta nada y
    /// devuelve false. Requiere una transacción abierta por el caller.
    /// </summary>
    internal async Task<bool> TryReserveAgainAsync(OrderEntity order, CancellationToken ct)
    {
        if (!order.StockReleased)
            return true;

        var tx = context.Database.CurrentTransaction
            ?? throw new InvalidOperationException("TryReserveAgainAsync requiere una transacción.");
        const string savepoint = "reserve_again";
        await tx.CreateSavepointAsync(savepoint, ct);

        foreach (var item in order.Items)
        {
            var reserved = await context.Product.IgnoreQueryFilters()
                .Where(p => p.Id == item.ProductId && p.Stock >= item.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - item.Quantity), ct);
            if (reserved == 0)
            {
                await tx.RollbackToSavepointAsync(savepoint, ct);
                return false;
            }
        }

        await tx.ReleaseSavepointAsync(savepoint, ct);
        order.StockReleased = false;
        return true;
    }
}
