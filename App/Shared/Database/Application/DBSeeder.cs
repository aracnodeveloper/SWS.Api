using App.Modules.Catalog;
using App.Modules.Role;
using Microsoft.EntityFrameworkCore;

namespace App.Shared.Database;

/// <summary>
/// Seed idempotente: roles base + catálogo 2026 (categorías y productos).
/// Ids fijos para que sea reproducible entre entornos. El usuario admin NO se
/// siembra aquí (su contraseña viene de configuración): ver EnsureAdminUser.
/// </summary>
public class DBSeeder
{
    public static readonly Guid ADMIN_ROLE_ID = Guid.Parse("51515151-0000-0000-0000-000000000001");
    public static readonly Guid CUSTOMER_ROLE_ID = Guid.Parse("51515151-0000-0000-0000-000000000002");

    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleEntity>().HasData(
            new RoleEntity
            {
                Id = ADMIN_ROLE_ID,
                Name = "Admin",
                Description = "Administrador de la tienda",
                Code = 1,
                IsActive = true,
                CreatedAt = SeedDate,
            },
            new RoleEntity
            {
                Id = CUSTOMER_ROLE_ID,
                Name = "Cliente",
                Description = "Cliente de la tienda",
                Code = 100,
                IsActive = true,
                CreatedAt = SeedDate,
            });

        CatalogSeed.Seed(modelBuilder, SeedDate);
    }
}
