using App.Modules.Logs;
using App.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Order;

/// <summary>
/// Cada 5 minutos expira pedidos pendientes cuya reserva venció y devuelve su stock.
/// PayPhone reversa solo los cobros que no confirmamos, así que expirar es seguro.
/// </summary>
public class ExpirePendingOrdersJob(IServiceScopeFactory scopeFactory, ILogger<ExpirePendingOrdersJob> log)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                log.LogError(e, "Error expirando pedidos pendientes");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var cancelOrder = scope.ServiceProvider.GetRequiredService<CancelOrder>();

        var now = DateTime.UtcNow;
        var expired = await context.Order
            .Include(o => o.Items)
            .Where(o => OrderStatus.Pending.Contains(o.Status) && o.ExpiresAt != null && o.ExpiresAt < now)
            .OrderBy(o => o.ExpiresAt)
            .Take(100)
            .ToListAsync(ct);

        foreach (var order in expired)
        {
            try
            {
                await cancelOrder.ExecuteAsync(order, OrderStatus.Expired, "Reserva vencida sin pago.", ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // El pago se confirmó justo ahora: se respeta ese resultado.
                context.ChangeTracker.Clear();
            }
        }
    }
}
