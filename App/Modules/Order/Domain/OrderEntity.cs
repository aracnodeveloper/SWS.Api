using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Shared;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Order;

/// <summary>Pedido de la tienda (compra como invitado: los datos del cliente viven aquí).</summary>
public class OrderEntity : IEntity, IAuditable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>Correlativo visible (SWS-000123). Identity en Postgres.</summary>
    public int Number { get; set; }

    /// <summary>
    /// Token secreto para consultar el pedido sin login (link de retorno del pago).
    /// Evita exponer el Id o permitir enumerar pedidos por número.
    /// </summary>
    public Guid PublicToken { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string Status { get; set; } = OrderStatus.PendingPayment;

    [Required, MaxLength(20)]
    public string PaymentMethod { get; set; } = PaymentMethods.PayPhone;

    // ── Cliente ──
    [Required, MaxLength(120)] public string CustomerName { get; set; } = null!;
    [Required, MaxLength(160)] public string Email { get; set; } = null!;
    [Required, MaxLength(20)] public string Phone { get; set; } = null!;
    /// <summary>Cédula o RUC (facturación).</summary>
    [Required, MaxLength(13)] public string DocumentId { get; set; } = null!;
    [Required, MaxLength(60)] public string Province { get; set; } = null!;
    [Required, MaxLength(80)] public string City { get; set; } = null!;
    [Required, MaxLength(250)] public string Address { get; set; } = null!;
    [MaxLength(250)] public string? AddressReference { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }

    // ── Totales (calculados en servidor, nunca vienen del cliente) ──
    [Precision(12, 2)] public decimal ItemsTotal { get; set; }
    [Precision(12, 2)] public decimal Shipping { get; set; }
    /// <summary>Base imponible.</summary>
    [Precision(12, 2)] public decimal Subtotal { get; set; }
    [Precision(12, 2)] public decimal Vat { get; set; }
    [Precision(12, 2)] public decimal Total { get; set; }

    /// <summary>Hasta cuándo se mantiene reservado el stock de un pedido pendiente.</summary>
    public DateTime? ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }

    /// <summary>true cuando el stock reservado ya volvió al inventario (evita devolverlo dos veces).</summary>
    public bool StockReleased { get; set; }

    /// <summary>Bitácora interna (cambios de estado, incidencias de pago).</summary>
    public string? AdminNotes { get; set; }

    public List<OrderItemEntity> Items { get; set; } = new();

    /// <summary>Concurrencia optimista (xmin): el job de expiración y la confirmación del pago no se pisan.</summary>
    [Timestamp]
    public uint Version { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? CreatedBy { get; set; }

    [NotMapped]
    public string Code => $"SWS-{Number:D6}";

    public void AppendNote(string note) =>
        AdminNotes = $"{AdminNotes}{(string.IsNullOrEmpty(AdminNotes) ? "" : "\n")}[{DateTime.UtcNow:yyyy-MM-dd HH:mm}Z] {note}";
}
