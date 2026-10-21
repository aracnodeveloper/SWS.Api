using App.Modules.Order;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.Extensions.Options;

namespace App.Modules.Payment;

/// <summary>Registra la transacción y pide a la pasarela la URL de pago del pedido.</summary>
public class StartPayment(
    ApplicationContext context,
    PaymentGatewayResolver resolver,
    IOptions<AppOptions> appOptions)
{
    public async Task<string> ExecuteAsync(OrderEntity order, CancellationToken ct)
    {
        var gateway = resolver.Resolve(order.PaymentMethod);
        var web = (appOptions.Value.PublicWebUrl ?? appOptions.Value.WebUrl.Split(',')[0]).TrimEnd('/');

        // Único y ≤ 50 caracteres (límite de PayPhone).
        var clientTxId = $"SWS{order.Number:D6}-{Guid.NewGuid():N}"[..24];

        var transaction = new TransactionEntity
        {
            OrderId = order.Id,
            Gateway = gateway.Name,
            ClientTransactionId = clientTxId,
            Amount = order.Total,
        };
        context.Transaction.Add(transaction);
        await context.SaveChangesAsync(ct);

        var request = new PaymentRequest(
            ClientTransactionId: clientTxId,
            AmountCents: ToCents(order.Total),
            AmountWithTaxCents: ToCents(order.Subtotal),
            AmountWithoutTaxCents: 0,
            TaxCents: ToCents(order.Vat),
            Reference: $"Pedido {order.Code} - Sale World Sale",
            Email: order.Email,
            ResponseUrl: $"{web}/pago/respuesta",
            CancellationUrl: $"{web}/pago/respuesta?cancel=1&clientTransactionId={clientTxId}&token={order.PublicToken}");

        try
        {
            var prepared = await gateway.PrepareAsync(request, ct);
            transaction.GatewayPaymentId = prepared.GatewayPaymentId;
            transaction.RawResponse = prepared.RawResponse;
            await context.SaveChangesAsync(ct);
            return prepared.RedirectUrl;
        }
        catch (PaymentGatewayException e)
        {
            transaction.Status = TransactionStatus.Failed;
            transaction.Message = Truncate(e.Message, 300);
            await context.SaveChangesAsync(ct);
            throw;
        }
    }

    internal static long ToCents(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
