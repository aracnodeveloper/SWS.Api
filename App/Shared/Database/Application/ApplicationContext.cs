using App.Modules.Catalog;
using App.Modules.Order;
using App.Modules.Payment;
using App.Modules.Role;
using App.Modules.User;
using App.Shared.Application;
using App.Shared.Database;
using App.Shared.Interceptor;
using App.Shared.ModelConfigurator;
using Microsoft.EntityFrameworkCore;

namespace App.Shared.Infrastructure;

public partial class ApplicationContext : DbContext
{
    // ── Identidad y acceso ──
    public DbSet<UserEntity> User => Set<UserEntity>();
    public DbSet<UserRoleEntity> UserRole => Set<UserRoleEntity>();
    public DbSet<RoleEntity> Role => Set<RoleEntity>();

    // ── Catálogo ──
    public DbSet<CategoryEntity> Category => Set<CategoryEntity>();
    public DbSet<ProductEntity> Product => Set<ProductEntity>();

    // ── Pedidos ──
    public DbSet<OrderEntity> Order => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItem => Set<OrderItemEntity>();

    // ── Pagos ──
    public DbSet<TransactionEntity> Transaction => Set<TransactionEntity>();

    protected readonly SessionAccesor _sessionAccesor;

    public ApplicationContext(DbContextOptions options, SessionAccesor sessionAccesor)
        : base(options)
    {
        _sessionAccesor = sessionAccesor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new SoftDeleteInterceptor(), new AuditableInterceptor());
        optionsBuilder.ConfigureWarnings(warnings =>
        {
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SoftDeleteDefaultConfigurator.Configure(modelBuilder);
        AuditableDefaultConfigurator.Configure(modelBuilder);

        // ── Columnas jsonb (respuesta cruda de la pasarela) ──
        modelBuilder.Entity<TransactionEntity>().Property(e => e.RawResponse).HasColumnType("jsonb");

        // ── Índices únicos ──
        modelBuilder.Entity<UserEntity>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<UserRoleEntity>().HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        modelBuilder.Entity<CategoryEntity>().HasIndex(c => c.Slug).IsUnique();
        modelBuilder.Entity<ProductEntity>().HasIndex(p => p.Slug).IsUnique();
        modelBuilder.Entity<ProductEntity>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<OrderEntity>().HasIndex(o => o.Number).IsUnique();
        modelBuilder.Entity<OrderEntity>().Property(o => o.Number).UseIdentityByDefaultColumn();
        modelBuilder.Entity<OrderEntity>().HasIndex(o => o.PublicToken).IsUnique();
        modelBuilder.Entity<OrderEntity>().HasIndex(o => new { o.Status, o.ExpiresAt });
        modelBuilder.Entity<TransactionEntity>().HasIndex(t => t.ClientTransactionId).IsUnique();

        // ── El stock no puede quedar negativo aunque falle la lógica de reserva ──
        modelBuilder.Entity<ProductEntity>()
            .ToTable(t => t.HasCheckConstraint("CK_Product_Stock_NonNegative", "\"Stock\" >= 0"));

        // ── Todas las FKs en Restrict: sin cascadas accidentales ni ciclos ──
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        new DBSeeder().Seed(modelBuilder);
    }
}
