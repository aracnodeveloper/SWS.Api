using System.Net;
using App.Modules.Logs;
using App.Shared;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.Modules.Order;

[ApiController]
[Route("api/[controller]")]
public class OrderController(
    ApplicationContext context,
    IAppLogger logger,
    UpdateOrderStatus updateOrderStatus,
    IOptions<StoreOptions> storeOptions)
    : BaseEntityController<OrderEntity>(context, logger, "Pedido")
{
    /// <summary>Consulta del cliente con el token secreto del link de retorno.</summary>
    [AllowAnonymous]
    [HttpGet("public/{token:guid}")]
    public async Task<ActionResult<PublicOrderDTO>> GetPublic(Guid token, CancellationToken ct)
    {
        var order = await repository.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.PublicToken == token, ct);
        return order is null
            ? ErrorMessage("Pedido no existe", HttpStatusCode.NotFound)
            : Ok(OrderMapper.ToPublic(order, storeOptions.Value));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = repository.AsNoTracking().Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var digits = new string(s.Where(char.IsDigit).ToArray());
            var number = int.TryParse(digits, out var n) ? n : -1;
            var term = $"%{s}%";
            query = query.Where(o => o.Number == number
                || EF.Functions.ILike(o.CustomerName, term)
                || EF.Functions.ILike(o.Email, term)
                || o.DocumentId == s);
        }
        return await ListEntities(query.OrderByDescending(o => o.Number), page, pageSize);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await repository.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return ErrorMessage("Pedido no existe", HttpStatusCode.NotFound);

        var transactions = await context.Transaction.AsNoTracking()
            .Where(t => t.OrderId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new { t.Id, t.Gateway, t.Status, t.Amount, t.AuthorizationCode, t.CardBrand, t.LastDigits, t.Message, t.CreatedAt })
            .ToListAsync(ct);

        return Ok(new { order, transactions });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> PatchStatus(Guid id, [FromBody] OrderStatusPatchDto dto, CancellationToken ct)
    {
        try
        {
            var (order, message) = await updateOrderStatus.ExecuteAsync(id, dto, ct);
            _logger.Log($"[Order] {order.Code} → {order.Status}", LogType.SUCCESS);
            return Ok(new HttpMessage(message));
        }
        catch (BadRequestException e)
        {
            return ErrorMessage(e.Message, HttpStatusCode.BadRequest);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ErrorMessage("El pedido cambió mientras lo editabas. Recarga e intenta de nuevo.", HttpStatusCode.Conflict);
        }
    }
}
