using System.Net;
using App.Modules.Logs;
using App.Modules.Payment;
using App.Shared.Bootstrap;
using App.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace App.Modules.Order;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CheckoutController(
    CreateOrder createOrder,
    CancelOrder cancelOrder,
    StartPayment startPayment,
    PaymentGatewayResolver gateways,
    OrderNotifier notifier,
    IOptions<StoreOptions> storeOptions,
    IAppLogger logger) : ControllerBase
{
    /// <summary>Reglas públicas que el front necesita para mostrar totales estimados.</summary>
    [HttpGet("config")]
    public ActionResult Config()
    {
        var s = storeOptions.Value;
        var methods = gateways.Available().Append(PaymentMethods.WhatsApp).ToList();
        return Ok(new
        {
            s.Name,
            s.Currency,
            s.VatRate,
            s.PricesIncludeVat,
            s.ShippingCost,
            s.FreeShippingFrom,
            s.WhatsAppNumber,
            s.ContactEmail,
            PaymentMethods = methods,
        });
    }

    [HttpPost]
    [EnableRateLimiting(Application.CheckoutRateLimit)]
    public async Task<ActionResult<CheckoutResponse>> Checkout(CheckoutRequest request, CancellationToken ct)
    {
        if (request.PaymentMethod != PaymentMethods.WhatsApp && !gateways.Available().Contains(request.PaymentMethod))
            return Error("Ese método de pago no está disponible por ahora. Elige pedido por WhatsApp.", HttpStatusCode.BadRequest);

        OrderEntity order;
        try
        {
            order = await createOrder.ExecuteAsync(request, ct);
        }
        catch (BadRequestException e)
        {
            return Error(e.Message, HttpStatusCode.BadRequest);
        }

        await notifier.NotifyNewOrderAsync(order, ct);

        var response = new CheckoutResponse
        {
            OrderCode = order.Code,
            PublicToken = order.PublicToken,
            PaymentMethod = order.PaymentMethod,
            Total = order.Total,
        };

        if (order.PaymentMethod == PaymentMethods.WhatsApp)
        {
            response.WhatsAppUrl = OrderMapper.WhatsAppUrl(order, storeOptions.Value);
            return Ok(response);
        }

        try
        {
            response.RedirectUrl = await startPayment.ExecuteAsync(order, ct);
            return Ok(response);
        }
        catch (PaymentGatewayException e)
        {
            // Sin URL de pago el pedido no sirve: se libera el stock de inmediato.
            logger.Log($"[Checkout] {order.Code}: no se pudo iniciar el pago ({e.Message})", LogType.ERROR);
            await cancelOrder.ExecuteAsync(order, OrderStatus.Cancelled, $"No se pudo iniciar el pago: {e.Message}", ct);
            return Error("No pudimos conectar con la pasarela de pago. Intenta de nuevo o pide por WhatsApp.",
                HttpStatusCode.BadGateway);
        }
    }

    private static ObjectResult Error(string message, HttpStatusCode status) =>
        new(new { message }) { StatusCode = (int)status };
}
