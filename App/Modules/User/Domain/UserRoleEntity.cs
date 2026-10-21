using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Modules.Role;
using App.Shared;

namespace App.Modules.User;

/// <summary>Pivote N:M entre users y roles.</summary>
public class UserRoleEntity : IEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }

    [ForeignKey(nameof(Role))]
    public Guid RoleId { get; set; }
    public RoleEntity? Role { get; set; }
}
