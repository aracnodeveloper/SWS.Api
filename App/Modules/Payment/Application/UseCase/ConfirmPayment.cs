using App.Modules.Logs;
using App.Modules.Order;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Payment;

/// <summary>
/// Confirma con la pasarela el resultado del pago al volver el cliente. Idempotente:
/// recargar la página de respuesta no vuelve a cobrar ni a cambiar el pedido.
/// </summary>
public class ConfirmPayment(
    ApplicationContext context,
    PaymentGatewayResolver resolver,
    CancelOrder cancelOrder,
    OrderNotifier notifier,
    IAppLogger logger)
{
    public async Task<OrderEntity> ExecuteAsync(string gatewayName, string gatewayId, string clientTxId, CancellationToken ct)
    {
        var transaction = await LoadAsync(clientTxId, gatewayName, ct);
        var order = transaction.Order!;

        // Ya resuelta antes (recarga de la página o doble llamada).
        if (transaction.Status is TransactionStatus.Approved or TransactionStatus.Canceled or TransactionStatus.Mismatch)
            return order;

        var result = await resolver.Resolve(gatewayName).ConfirmAsync(gatewayId, clientTxId, ct);

        transaction.GatewayTransactionId = result.TransactionId;
        transaction.AuthorizationCode = result.AuthorizationCode;
        transaction.CardBrand = result.CardBrand;
        transaction.LastDigits = result.LastDigits;
        transaction.Message = result.Message?.Length > 300 ? result.Message[..300] : result.Message;
        transaction.RawResponse = result.RawResponse;

        var expectedCents = StartPayment.ToCents(transaction.Amount);
        if (result.Approved && (result.ClientTransactionId != clientTxId || result.AmountCents != expectedCents))
        {
            transaction.Status = TransactionStatus.Mismatch;
            // El cobro ya se confirmó en la pasarela: no dejar que el job lo expire.
            order.ExpiresAt = null;
            order.AppendNote($"ALERTA: pago {result.TransactionId} con monto/referencia distintos " +
                             $"({result.AmountCents} vs {expectedCents} centavos). Revisar en PayPhone.");
            await context.SaveChangesAsync(ct);
            logger.Log($"[Payment] Mismatch en {order.Code}: {result.RawResponse}", LogType.ERROR);
            return order;
        }

        if (!result.Approved)
        {
            transaction.Status = TransactionStatus.Canceled;
            if (OrderStatus.Pending.Contains(order.Status))
                await cancelOrder.ExecuteAsync(order, OrderStatus.Cancelled, $"Pago no aprobado: {result.Message}", ct);
            else
                await context.SaveChangesAsync(ct);
            return order;
        }

        await using (var tx = await context.Database.BeginTransactionAsync(ct))
        {
            transaction.Status = TransactionStatus.Approved;
            if (order.StockReleased && !await cancelOrder.TryReserveAgainAsync(order, ct))
                order.AppendNote("ALERTA: pago aprobado después de expirar la reserva y ya no hay stock suficiente. " +
                                 "Contactar al cliente o anular el cobro.");

            order.Status = OrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow;
            order.ExpiresAt = null;
            order.AppendNote($"Pago {gatewayName} aprobado. Autorización {result.AuthorizationCode}, transacción {result.TransactionId}.");
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        logger.Log($"[Payment] {order.Code} pagado vía {gatewayName} ({result.TransactionId}).", LogType.SUCCESS);
        await notifier.NotifyPaidAsync(order, ct);
        return order;
    }

    /// <summary>El cliente canceló en la pasarela: libera la reserva de inmediato.</summary>
    public async Task<OrderEntity> CancelAsync(string clientTxId, Guid publicToken, CancellationToken ct)
    {
        var transaction = await LoadAsync(clientTxId, null, ct);
        var order = transaction.Order!;
        if (order.PublicToken != publicToken)
            throw new BadRequestException("Pedido no encontrado.");

        if (transaction.Status == TransactionStatus.Created)
        {
            transaction.Status = TransactionStatus.Canceled;
            transaction.Message = "Cancelado por el cliente.";
            if (OrderStatus.Pending.Contains(order.Status))
                await cancelOrder.ExecuteAsync(order, OrderStatus.Cancelled, "El cliente canceló el pago.", ct);
            else
                await context.SaveChangesAsync(ct);
        }
        return order;
    }

    private async Task<TransactionEntity> LoadAsync(string clientTxId, string? gatewayName, CancellationToken ct) =>
        await context.Transaction
            .Include(t => t.Order).ThenInclude(o => o!.Items)
            .FirstOrDefaultAsync(t => t.ClientTransactionId == clientTxId
                                      && (gatewayName == null || t.Gateway == gatewayName), ct)
        ?? throw new BadRequestException("Transacción no encontrada.");
}
