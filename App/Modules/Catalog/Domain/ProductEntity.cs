using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Shared;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Catalog;

public class ProductEntity : IEntity, IEntityStatus, IAuditable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required, MaxLength(40)]
    public string Sku { get; set; } = null!;

    /// <summary>Identificador para URL (/producto/airpods-5).</summary>
    [Required, MaxLength(120)]
    public string Slug { get; set; } = null!;

    [Required, MaxLength(160)]
    public string Name { get; set; } = null!;

    [MaxLength(80)]
    public string? Brand { get; set; }

    [MaxLength(400)]
    public string? Description { get; set; }

    /// <summary>Viñetas del catálogo (características / obsequios).</summary>
    public List<string> Features { get; set; } = new();

    /// <summary>Precio de venta publicado (con IVA si Store:PricesIncludeVat).</summary>
    [Precision(12, 2)]
    public decimal Price { get; set; }

    /// <summary>Precio anterior tachado (opcional, para promociones).</summary>
    [Precision(12, 2)]
    public decimal? CompareAtPrice { get; set; }

    /// <summary>Unidades disponibles. Se descuenta al crear el pedido (reserva).</summary>
    public int Stock { get; set; }

    /// <summary>
    /// Ruta de la imagen principal. "/products/x.webp" = asset del front;
    /// "/uploads/products/x.webp" = subida al API desde el admin.
    /// </summary>
    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    /// <summary>Etiqueta destacada ("3 obsequios", "Nuevo").</summary>
    [MaxLength(40)]
    public string? Badge { get; set; }

    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }

    [ForeignKey(nameof(Category))]
    public Guid CategoryId { get; set; }
    public CategoryEntity? Category { get; set; }

    public bool IsStatus { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? CreatedBy { get; set; }
}
