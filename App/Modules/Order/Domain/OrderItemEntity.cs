using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Modules.Catalog;
using App.Shared;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Order;

/// <summary>Línea del pedido. Guarda snapshot de nombre/precio por si el producto cambia después.</summary>
public class OrderItemEntity : IEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }
    public OrderEntity? Order { get; set; }

    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }
    public ProductEntity? Product { get; set; }

    [Required, MaxLength(40)] public string Sku { get; set; } = null!;
    [Required, MaxLength(160)] public string ProductName { get; set; } = null!;
    [MaxLength(300)] public string? ImageUrl { get; set; }

    [Precision(12, 2)] public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    [Precision(12, 2)] public decimal LineTotal { get; set; }
}
