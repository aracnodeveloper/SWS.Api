using Microsoft.EntityFrameworkCore;

namespace App.Modules.Catalog;

/// <summary>
/// Catálogo de Productos 2026 (PDF). Las imágenes viven en el front (/products/*.webp);
/// el admin puede reemplazarlas subiendo una nueva (queda en /uploads/products).
/// El stock inicial es un valor de arranque: ajustarlo desde el admin.
/// </summary>
internal static class CatalogSeed
{
    private const int InitialStock = 5;

    private static Guid C(int n) => Guid.Parse($"5a5c0000-0000-0000-0000-{n:D12}");
    private static Guid P(int n) => Guid.Parse($"5a5b0000-0000-0000-0000-{n:D12}");

    public static void Seed(ModelBuilder modelBuilder, DateTime seedDate)
    {
        modelBuilder.Entity<CategoryEntity>().HasData(
            new CategoryEntity { Id = C(1), Slug = "streaming", Name = "Streaming", Description = "Lleva el entretenimiento a cualquier televisor", SortOrder = 1 },
            new CategoryEntity { Id = C(2), Slug = "hogar-inteligente", Name = "Hogar inteligente", Description = "Alexa en tu casa, en dos colores", SortOrder = 2 },
            new CategoryEntity { Id = C(3), Slug = "audio", Name = "Audio", Description = "Sonido inalámbrico de calidad", SortOrder = 3 },
            new CategoryEntity { Id = C(4), Slug = "cables-apple", Name = "Cables Apple", Description = "Carga y sincroniza con cables originales", SortOrder = 4 },
            new CategoryEntity { Id = C(5), Slug = "carga-rapida", Name = "Carga rápida", Description = "Cargador y cable de alto rendimiento", SortOrder = 5 },
            new CategoryEntity { Id = C(6), Slug = "accesorios", Name = "Accesorios", Description = "Protege tus dispositivos con estilo", SortOrder = 6 },
            new CategoryEntity { Id = C(7), Slug = "mas-para-ti", Name = "Más para ti", Description = "Localizador y mochila", SortOrder = 7 });

        var products = new[]
        {
            Product(1, 1, "SWS-001", "fire-tv-stick-4k-select", "Amazon Fire TV Stick 4K Select", "Amazon", 45m, featured: true,
                "Convierte cualquier televisor en un Smart TV con resolución 4K. Incluye control remoto por voz con Alexa.",
                "Resolución 4K Ultra HD", "Control remoto con Alexa", "Fácil instalación por HDMI", "Stock limitado"),
            Product(2, 1, "SWS-002", "fire-tv-stick-hd", "Amazon Fire TV Stick HD", "Amazon", 40m, featured: false,
                "Accede a tus apps de streaming favoritas en alta definición, con conexión rápida y estable.",
                "8 GB de almacenamiento", "Wi-Fi 6", "Control remoto con Alexa", "Stock limitado"),
            Product(3, 2, "SWS-003", "echo-dot-5-carbon", "Echo Dot 5.ª generación · Carbón", "Amazon", 65m, featured: true,
                "Parlante inteligente con Alexa: música, recordatorios y control de tu hogar con la voz.",
                "Asistente de voz Alexa", "Sonido mejorado", "Control de dispositivos del hogar", "Stock limitado"),
            Product(4, 2, "SWS-004", "echo-dot-5-azul", "Echo Dot 5.ª generación · Azul", "Amazon", 65m, featured: false,
                "El mismo Echo Dot con Alexa en color azul, ideal para cualquier espacio de tu casa.",
                "Asistente de voz Alexa", "Sonido mejorado", "Control de dispositivos del hogar", "Stock limitado"),
            Product(5, 3, "SWS-005", "airpods-5", "AirPods 5", "Apple", 250m, featured: true,
                "Audífonos inalámbricos Apple con estuche de carga inalámbrica. ¡Incluyen 3 obsequios!",
                "Estuche de carga inalámbrica", "Obsequio: case de silicona", "Obsequio: kit de limpieza", "Obsequio: cable USB-C a USB-C"),
            Product(6, 3, "SWS-006", "jbl-tune-flex-2", "JBL Tune Flex 2", "JBL", 60m, featured: true,
                "Audífonos inalámbricos con sonido JBL Pure Bass, sonido espacial y cancelación de ruido adaptable.",
                "Cancelación de ruido adaptable", "Hasta 48 horas de reproducción", "Resistentes al agua y polvo IP54", "6 micrófonos para llamadas"),
            Product(7, 4, "SWS-007", "cable-apple-usbc-usbc-60w", "Cable Apple USB-C a USB-C 60 W", "Apple", 25m, featured: false,
                "Cable original Apple de 1 metro para carga y transferencia de datos de iPhone, iPad y Mac.",
                "Potencia de hasta 60 W", "Longitud: 1 m", "Conectores USB-C en ambos extremos", "Original Apple"),
            Product(8, 4, "SWS-008", "cable-apple-usbc-lightning", "Cable Apple USB-C a Lightning", "Apple", 25m, featured: false,
                "Cable original Apple de 1 metro para cargar y sincronizar iPhone, iPad y AirPods con conector Lightning.",
                "Conectores USB-C y Lightning", "Longitud: 1 m", "Compatible con carga rápida", "Original Apple"),
            Product(9, 5, "SWS-009", "cubo-apple-usbc-20w", "Cubo Apple USB-C 20 W", "Apple", 25m, featured: true,
                "Adaptador de corriente original Apple con carga rápida para iPhone, iPad y AirPods.",
                "Carga rápida de 20 W", "Puerto USB-C", "Compacto y ligero", "Original Apple"),
            Product(10, 5, "SWS-010", "cable-beats-usbc-60w", "Cable Beats USB-C a USB-C 60 W", "Beats", 25m, featured: false,
                "Cable trenzado resistente, compatible con iPhone, iPad, Android, parlantes y más.",
                "Potencia de hasta 60 W", "Compatibilidad universal USB-C", "Diseño resistente", "Original Beats"),
            Product(11, 6, "SWS-011", "case-airpods-4-kit-limpieza", "Case AirPods 4 + Kit de Limpieza", null, 10m, featured: false,
                "Funda de silicona para AirPods de 4.ª generación con kit de limpieza y mosquetón incluidos.",
                "Protección de silicona", "Kit de limpieza incluido", "Mosquetón para llevarlo contigo", "Compacto y dinámico"),
            Product(12, 6, "SWS-012", "carcasa-macbook-neo-13", "Carcasa MacBook Neo 13\"", null, 25m, featured: false,
                "Carcasa rígida ultradelgada y transparente que se coloca a presión y protege sin ocultar el diseño.",
                "Ultradelgada y transparente", "Instalación a presión", "Para MacBook Neo de 13 pulgadas", "Stock limitado"),
            Product(13, 7, "SWS-013", "ugreen-finetrack", "UGREEN FineTrack", "UGREEN", 15m, featured: true,
                "Localizador certificado para Apple Buscar (Find My). Ubica llaves, mochilas o maletas desde tu iPhone.",
                "Compatible con Apple Buscar", "Funciona con Siri", "Compacto y ligero", "No compatible con Android"),
            Product(14, 7, "SWS-014", "mochila-puma-meridian", "Mochila Puma Meridian", "Puma", 40m, featured: false,
                "Mochila original Puma con amplio compartimento principal y bolsillos frontales con cierre.",
                "Bolsillos frontales con cierre", "Bolsillos laterales de malla", "Diseño deportivo", "Original Puma"),
        };
        products[4].Badge = "3 obsequios";

        foreach (var p in products)
            p.CreatedAt = seedDate;

        modelBuilder.Entity<ProductEntity>().HasData(products);
    }

    private static ProductEntity Product(
        int n, int category, string sku, string slug, string name, string? brand, decimal price,
        bool featured, string description, params string[] features) => new()
    {
        Id = P(n),
        CategoryId = C(category),
        Sku = sku,
        Slug = slug,
        Name = name,
        Brand = brand,
        Price = price,
        Description = description,
        Features = features.ToList(),
        ImageUrl = $"/products/{slug}.webp",
        Stock = InitialStock,
        IsFeatured = featured,
        SortOrder = n,
    };
}
