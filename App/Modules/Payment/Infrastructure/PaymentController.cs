using System.Net;
using App.Modules.Logs;
using App.Modules.Order;
using App.Shared.Bootstrap;
using App.Shared.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.Modules.Payment;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting(Application.CheckoutRateLimit)]
public class PaymentController(
    ConfirmPayment confirmPayment,
    IOptions<StoreOptions> storeOptions,
    IAppLogger logger) : ControllerBase
{
    /// <summary>La página /pago/respuesta reenvía aquí los parámetros que agrega PayPhone.</summary>
    [HttpPost("payphone/confirm")]
    public async Task<ActionResult<PublicOrderDTO>> ConfirmPayPhone(PayPhoneConfirmRequest request, CancellationToken ct)
    {
        try
        {
            var order = await confirmPayment.ExecuteAsync(
                PayPhoneGateway.GatewayName, request.Id.ToString(), request.ClientTransactionId, ct);
            return Ok(OrderMapper.ToPublic(order, storeOptions.Value));
        }
        catch (BadRequestException e)
        {
            return Error(e.Message, HttpStatusCode.BadRequest);
        }
        catch (PaymentGatewayException e)
        {
            return Error($"No pudimos confirmar el pago con PayPhone: {e.Message}", HttpStatusCode.BadGateway);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.Log($"[Payment] Confirmación concurrente de {request.ClientTransactionId}", LogType.WARNING);
            return Error("El pedido se está actualizando. Recarga la página en unos segundos.", HttpStatusCode.Conflict);
        }
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<PublicOrderDTO>> Cancel(PaymentCancelRequest request, CancellationToken ct)
    {
        try
        {
            var order = await confirmPayment.CancelAsync(request.ClientTransactionId, request.Token, ct);
            return Ok(OrderMapper.ToPublic(order, storeOptions.Value));
        }
        catch (BadRequestException e)
        {
            return Error(e.Message, HttpStatusCode.BadRequest);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error("El pedido se está actualizando. Recarga la página en unos segundos.", HttpStatusCode.Conflict);
        }
    }

    private static ObjectResult Error(string message, HttpStatusCode status) =>
        new(new { message }) { StatusCode = (int)status };
}
