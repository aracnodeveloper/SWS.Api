using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Order;

/// <summary>Cambio de estado manual desde el admin (confirmar WhatsApp, despachar, cancelar...).</summary>
public class UpdateOrderStatus(ApplicationContext context, CancelOrder cancelOrder, OrderNotifier notifier)
{
    public async Task<(OrderEntity Order, string Message)> ExecuteAsync(
        Guid orderId, OrderStatusPatchDto dto, CancellationToken ct)
    {
        var order = await context.Order.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new BadRequestException("Pedido no existe.");

        if (!OrderStatus.AllowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new BadRequestException($"No se puede pasar de \"{order.Status}\" a \"{dto.Status}\".");

        var note = $"Admin: {order.Status} → {dto.Status}" + (string.IsNullOrWhiteSpace(dto.Note) ? "" : $" ({dto.Note.Trim()})");
        var message = "Estado actualizado";

        if (dto.Status == OrderStatus.Cancelled)
        {
            var wasCharged = order.Status != OrderStatus.PendingPayment && order.PaidAt is not null
                && order.PaymentMethod == PaymentMethods.PayPhone;
            await cancelOrder.ExecuteAsync(order, OrderStatus.Cancelled, note, ct);
            if (wasCharged)
                message = "Pedido cancelado y stock devuelto. Recuerda anular el cobro desde el panel de PayPhone.";
            return (order, message);
        }

        var becomesPaid = dto.Status == OrderStatus.Paid;
        order.Status = dto.Status;
        order.AppendNote(note);
        if (becomesPaid)
        {
            order.PaidAt = DateTime.UtcNow;
            order.ExpiresAt = null;
        }
        await context.SaveChangesAsync(ct);

        if (becomesPaid)
            await notifier.NotifyPaidAsync(order, ct);

        return (order, message);
    }
}
