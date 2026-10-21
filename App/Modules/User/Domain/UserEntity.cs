using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Shared;

namespace App.Modules.User;

/// <summary>
/// Cuenta de acceso (administradores; a futuro clientes registrados).
/// Borrado físico (no implementa IEntityStatus).
/// </summary>
public class UserEntity : IEntity, IAuditable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>Email (login).</summary>
    [Required, MaxLength(160)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    /// <summary>Hash PBKDF2 de la contraseña (nunca texto plano).</summary>
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Auditable ──
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? CreatedBy { get; set; }
}
