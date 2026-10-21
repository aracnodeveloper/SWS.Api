using App.Modules.Logs;
using App.Modules.User;
using App.Shared.Database;
using App.Shared.Domain;
using App.Shared.Encrypt;
using App.Shared.Infrastructure;
using Microsoft.Extensions.Options;

namespace App.Modules.Auth;

/// <summary>
/// Crea el administrador inicial desde configuración (Admin:Email / Admin:Password)
/// solo si la tabla de usuarios está vacía. Así no hay contraseñas por defecto en el
/// código ni en las migraciones.
/// </summary>
public class EnsureAdminUser(
    ApplicationContext context,
    PasswordHasher passwordHasher,
    IOptions<AdminOptions> options,
    IAppLogger logger)
{
    public void Execute()
    {
        if (context.User.Any())
            return;

        var admin = options.Value;
        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password))
        {
            logger.Log("[Auth] No hay usuarios y Admin:Email/Admin:Password no están configurados; " +
                       "el panel admin quedará inaccesible.", LogType.WARNING);
            return;
        }

        var user = new UserEntity
        {
            Email = admin.Email.Trim().ToLowerInvariant(),
            Name = admin.Name,
            Password = passwordHasher.ComputeHash(admin.Password),
            IsActive = true,
        };
        context.User.Add(user);
        context.UserRole.Add(new UserRoleEntity { User = user, RoleId = DBSeeder.ADMIN_ROLE_ID });
        context.SaveChanges();
        logger.Log($"[Auth] Administrador inicial creado: {user.Email}", LogType.SUCCESS);
    }
}
