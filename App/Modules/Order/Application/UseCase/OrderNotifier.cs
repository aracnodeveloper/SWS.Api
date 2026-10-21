using System.Globalization;
using System.Net;
using System.Text;
using App.Modules.Logs;
using App.Shared.Domain;
using App.Shared.Email;
using Microsoft.Extensions.Options;

namespace App.Modules.Order;

/// <summary>
/// Correos transaccionales. Best-effort: si el SMTP no está configurado o falla,
/// se registra y el flujo del pedido sigue.
/// </summary>
public class OrderNotifier(
    IEmailSender emailSender,
    IOptions<StoreOptions> storeOptions,
    IOptions<AppOptions> appOptions,
    IAppLogger logger)
{
    private static readonly CultureInfo Money = CultureInfo.InvariantCulture;

    public async Task NotifyNewOrderAsync(OrderEntity order, CancellationToken ct)
    {
        var store = storeOptions.Value;
        if (string.IsNullOrWhiteSpace(store.ContactEmail))
            return;
        await SendAsync(store.ContactEmail, $"Nuevo pedido {order.Code} ({order.PaymentMethod})",
            Body(order, "Nuevo pedido recibido", $"Cliente: {order.CustomerName} · {order.Phone} · {order.Email}"), ct);
    }

    public async Task NotifyPaidAsync(OrderEntity order, CancellationToken ct)
    {
        var store = storeOptions.Value;
        var link = $"{appOptions.Value.PublicWebUrl?.TrimEnd('/')}/pedido/{order.PublicToken}";
        await SendAsync(order.Email, $"Tu pedido {order.Code} está confirmado",
            Body(order, $"¡Gracias por tu compra, {order.CustomerName.Split(' ')[0]}!",
                $"Te contactaremos por WhatsApp para coordinar la entrega. Consulta tu pedido aquí: {link}"), ct);
        if (!string.IsNullOrWhiteSpace(store.ContactEmail))
            await SendAsync(store.ContactEmail, $"Pedido pagado {order.Code}",
                Body(order, "Pedido pagado — preparar despacho",
                    $"Cliente: {order.CustomerName} · {order.Phone} · {order.Email}<br>Entrega: {order.Address}, {order.City}, {order.Province}"), ct);
    }

    private async Task SendAsync(string to, string subject, string html, CancellationToken ct)
    {
        try
        {
            await emailSender.SendAsync(to, subject, html, ct);
        }
        catch (Exception e)
        {
            logger.Log($"[Mail] No se envió \"{subject}\" a {to}: {e.Message}", LogType.WARNING);
        }
    }

    private static string Body(OrderEntity order, string title, string intro)
    {
        var rows = new StringBuilder();
        foreach (var i in order.Items)
            rows.Append($"<tr><td style=\"padding:6px 0\">{i.Quantity} × {WebUtility.HtmlEncode(i.ProductName)}</td>" +
                        $"<td style=\"text-align:right\">${i.LineTotal.ToString("0.00", Money)}</td></tr>");

        return $"""
            <div style="font-family:Arial,sans-serif;max-width:560px;margin:auto;color:#0a1a4f">
              <h2 style="color:#3d63f5">{WebUtility.HtmlEncode(title)}</h2>
              <p>{intro}</p>
              <p><strong>Pedido {order.Code}</strong></p>
              <table style="width:100%;border-collapse:collapse">{rows}
                <tr><td style="padding-top:8px;border-top:1px solid #dde">Envío</td><td style="text-align:right;border-top:1px solid #dde">${order.Shipping.ToString("0.00", Money)}</td></tr>
                <tr><td>IVA incluido</td><td style="text-align:right">${order.Vat.ToString("0.00", Money)}</td></tr>
                <tr><td><strong>Total</strong></td><td style="text-align:right"><strong>${order.Total.ToString("0.00", Money)}</strong></td></tr>
              </table>
              <p style="color:#667;font-size:12px;margin-top:24px">Sale World Sale · Innovando para Ti...</p>
            </div>
            """;
    }
}
