using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Shared;

namespace App.Modules.Catalog;

/// <summary>Sección del catálogo (Streaming, Audio, Cables Apple...).</summary>
public class CategoryEntity : IEntity, IEntityStatus
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>Identificador para URL (/catalogo?categoria=audio).</summary>
    [Required, MaxLength(80)]
    public string Slug { get; set; } = null!;

    [Required, MaxLength(80)]
    public string Name { get; set; } = null!;

    /// <summary>Subtítulo de la sección ("Sonido inalámbrico de calidad").</summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsStatus { get; set; } = true;
}
