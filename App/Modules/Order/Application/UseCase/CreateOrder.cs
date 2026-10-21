using App.Modules.Logs;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.Modules.Order;

/// <summary>
/// Crea el pedido y reserva stock de forma atómica: cada descuento es un
/// UPDATE ... WHERE Stock >= cantidad, así dos clientes no pueden llevarse la última unidad.
/// </summary>
public class CreateOrder(ApplicationContext context, IOptions<StoreOptions> storeOptions, IAppLogger logger)
{
    public async Task<OrderEntity> ExecuteAsync(CheckoutRequest request, CancellationToken ct)
    {
        var store = storeOptions.Value;
        if (!PaymentMethods.All.Contains(request.PaymentMethod))
            throw new BadRequestException("Método de pago no válido.");

        var lines = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        var ids = lines.Select(l => l.ProductId).ToList();
        var products = await context.Product.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        if (products.Count != lines.Count)
            throw new BadRequestException("Uno de los productos ya no está disponible. Actualiza tu carrito.");

        await using var tx = await context.Database.BeginTransactionAsync(ct);

        var order = new OrderEntity
        {
            PaymentMethod = request.PaymentMethod,
            Status = request.PaymentMethod == PaymentMethods.WhatsApp
                ? OrderStatus.PendingConfirmation
                : OrderStatus.PendingPayment,
            ExpiresAt = request.PaymentMethod == PaymentMethods.WhatsApp
                ? DateTime.UtcNow.AddHours(store.WhatsAppHoldHours)
                : DateTime.UtcNow.AddMinutes(store.PendingPaymentMinutes),
            CustomerName = request.CustomerName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Replace(" ", ""),
            DocumentId = request.DocumentId.Trim(),
            Province = request.Province.Trim(),
            City = request.City.Trim(),
            Address = request.Address.Trim(),
            AddressReference = request.AddressReference?.Trim(),
            Notes = request.Notes?.Trim(),
        };

        foreach (var line in lines)
        {
            var product = products[line.ProductId];
            var reserved = await context.Product
                .Where(p => p.Id == line.ProductId && p.Stock >= line.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - line.Quantity), ct);

            if (reserved == 0)
            {
                // Releer: otro pedido pudo reservar entre la carga inicial y este UPDATE.
                var available = await context.Product.Where(p => p.Id == line.ProductId)
                    .Select(p => p.Stock).FirstOrDefaultAsync(ct);
                throw new BadRequestException(available > 0
                    ? $"Solo quedan {available} unidades de \"{product.Name}\"."
                    : $"\"{product.Name}\" se agotó.");
            }

            order.Items.Add(new OrderItemEntity
            {
                ProductId = product.Id,
                Sku = product.Sku,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = line.Quantity,
                LineTotal = product.Price * line.Quantity,
            });
        }

        var totals = OrderPricing.Calculate(order.Items.Sum(i => i.LineTotal), store);
        order.ItemsTotal = totals.ItemsTotal;
        order.Shipping = totals.Shipping;
        order.Subtotal = totals.Subtotal;
        order.Vat = totals.Vat;
        order.Total = totals.Total;

        context.Order.Add(order);
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.Log($"[Order] {order.Code} creado ({order.PaymentMethod}) por ${order.Total}.", LogType.SUCCESS);
        return order;
    }
}
