using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Modules.Order;
using App.Shared;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Payment;

/// <summary>Cada intento de cobro en una pasarela (un pedido puede tener varios).</summary>
public class TransactionEntity : IEntity, IAuditable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }
    public OrderEntity? Order { get; set; }

    /// <summary>payphone | (kushki, datafast... a futuro)</summary>
    [Required, MaxLength(20)]
    public string Gateway { get; set; } = null!;

    /// <summary>Id que generamos nosotros y la pasarela nos devuelve (único).</summary>
    [Required, MaxLength(50)]
    public string ClientTransactionId { get; set; } = null!;

    /// <summary>Id del intento en la pasarela (PayPhone: paymentId del Prepare).</summary>
    [MaxLength(80)]
    public string? GatewayPaymentId { get; set; }

    /// <summary>Id de la transacción aprobada/rechazada (PayPhone: transactionId).</summary>
    [MaxLength(80)]
    public string? GatewayTransactionId { get; set; }

    [MaxLength(40)] public string? AuthorizationCode { get; set; }
    [MaxLength(40)] public string? CardBrand { get; set; }
    [MaxLength(8)] public string? LastDigits { get; set; }

    [Precision(12, 2)]
    public decimal Amount { get; set; }

    /// <summary>created | approved | canceled | failed | mismatch</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = TransactionStatus.Created;

    [MaxLength(300)]
    public string? Message { get; set; }

    /// <summary>Respuesta cruda de la pasarela (jsonb).</summary>
    public string? RawResponse { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? CreatedBy { get; set; }
}

public static class TransactionStatus
{
    public const string Created = "created";
    public const string Approved = "approved";
    public const string Canceled = "canceled";
    public const string Failed = "failed";

    /// <summary>La pasarela respondió con monto o referencia distintos a los esperados.</summary>
    public const string Mismatch = "mismatch";
}
