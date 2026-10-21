using System.Globalization;
using System.Text;
using App.Shared.Domain;

namespace App.Modules.Order;

public static class OrderMapper
{
    private static readonly CultureInfo Money = CultureInfo.InvariantCulture;

    public static PublicOrderDTO ToPublic(OrderEntity order, StoreOptions store) => new()
    {
        Code = order.Code,
        PublicToken = order.PublicToken,
        Status = order.Status,
        PaymentMethod = order.PaymentMethod,
        CustomerName = order.CustomerName,
        Email = order.Email,
        City = order.City,
        ItemsTotal = order.ItemsTotal,
        Shipping = order.Shipping,
        Subtotal = order.Subtotal,
        Vat = order.Vat,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        PaidAt = order.PaidAt,
        WhatsAppUrl = order.Status == OrderStatus.PendingConfirmation ? WhatsAppUrl(order, store) : null,
        Items = order.Items.Select(i => new PublicOrderItemDTO
        {
            ProductName = i.ProductName,
            ImageUrl = i.ImageUrl,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal,
        }).ToList(),
    };

    /// <summary>Link wa.me con el resumen del pedido para que el cliente lo envíe.</summary>
    public static string WhatsAppUrl(OrderEntity order, StoreOptions store)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Hola {store.Name}, quiero confirmar mi pedido *{order.Code}*:");
        foreach (var item in order.Items)
            sb.AppendLine($"• {item.Quantity} x {item.ProductName} — ${item.LineTotal.ToString("0.00", Money)}");
        if (order.Shipping > 0)
            sb.AppendLine($"Envío: ${order.Shipping.ToString("0.00", Money)}");
        sb.AppendLine($"*Total: ${order.Total.ToString("0.00", Money)}*");
        sb.AppendLine();
        sb.AppendLine($"Nombre: {order.CustomerName}");
        sb.AppendLine($"Entrega: {order.City}, {order.Province}");

        return $"https://wa.me/{store.WhatsAppNumber}?text={Uri.EscapeDataString(sb.ToString().TrimEnd())}";
    }
}
