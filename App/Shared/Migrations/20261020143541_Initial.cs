using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.App.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsStatus = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicToken = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DocumentId = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Province = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    City = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AddressReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ItemsTotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Shipping = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StockReleased = table.Column<bool>(type: "boolean", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Brand = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Features = table.Column<List<string>>(type: "text[]", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CompareAtPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Badge = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsStatus = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.CheckConstraint("CK_Product_Stock_NonNegative", "\"Stock\" >= 0");
                    table.ForeignKey(
                        name: "FK_Product_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gateway = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientTransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GatewayPaymentId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    GatewayTransactionId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AuthorizationCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CardBrand = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LastDigits = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RawResponse = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "Id", "Description", "IsStatus", "Name", "Slug", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("5a5c0000-0000-0000-0000-000000000001"), "Lleva el entretenimiento a cualquier televisor", true, "Streaming", "streaming", 1 },
                    { new Guid("5a5c0000-0000-0000-0000-000000000002"), "Alexa en tu casa, en dos colores", true, "Hogar inteligente", "hogar-inteligente", 2 },
                    { new Guid("5a5c0000-0000-0000-0000-000000000003"), "Sonido inalámbrico de calidad", true, "Audio", "audio", 3 },
                    { new Guid("5a5c0000-0000-0000-0000-000000000004"), "Carga y sincroniza con cables originales", true, "Cables Apple", "cables-apple", 4 },
                    { new Guid("5a5c0000-0000-0000-0000-000000000005"), "Cargador y cable de alto rendimiento", true, "Carga rápida", "carga-rapida", 5 },
                    { new Guid("5a5c0000-0000-0000-0000-000000000006"), "Protege tus dispositivos con estilo", true, "Accesorios", "accesorios", 6 },
                    { new Guid("5a5c0000-0000-0000-0000-000000000007"), "Localizador y mochila", true, "Más para ti", "mas-para-ti", 7 }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("51515151-0000-0000-0000-000000000001"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Administrador de la tienda", true, "Admin", null, null },
                    { new Guid("51515151-0000-0000-0000-000000000002"), 100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cliente de la tienda", true, "Cliente", null, null }
                });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "Badge", "Brand", "CategoryId", "CompareAtPrice", "CreatedAt", "CreatedBy", "Description", "Features", "ImageUrl", "IsFeatured", "IsStatus", "Name", "Price", "Sku", "Slug", "SortOrder", "Stock", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("5a5b0000-0000-0000-0000-000000000001"), null, "Amazon", new Guid("5a5c0000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Convierte cualquier televisor en un Smart TV con resolución 4K. Incluye control remoto por voz con Alexa.", new List<string> { "Resolución 4K Ultra HD", "Control remoto con Alexa", "Fácil instalación por HDMI", "Stock limitado" }, "/products/fire-tv-stick-4k-select.webp", true, true, "Amazon Fire TV Stick 4K Select", 45m, "SWS-001", "fire-tv-stick-4k-select", 1, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000002"), null, "Amazon", new Guid("5a5c0000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Accede a tus apps de streaming favoritas en alta definición, con conexión rápida y estable.", new List<string> { "8 GB de almacenamiento", "Wi-Fi 6", "Control remoto con Alexa", "Stock limitado" }, "/products/fire-tv-stick-hd.webp", false, true, "Amazon Fire TV Stick HD", 40m, "SWS-002", "fire-tv-stick-hd", 2, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000003"), null, "Amazon", new Guid("5a5c0000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Parlante inteligente con Alexa: música, recordatorios y control de tu hogar con la voz.", new List<string> { "Asistente de voz Alexa", "Sonido mejorado", "Control de dispositivos del hogar", "Stock limitado" }, "/products/echo-dot-5-carbon.webp", true, true, "Echo Dot 5.ª generación · Carbón", 65m, "SWS-003", "echo-dot-5-carbon", 3, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000004"), null, "Amazon", new Guid("5a5c0000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "El mismo Echo Dot con Alexa en color azul, ideal para cualquier espacio de tu casa.", new List<string> { "Asistente de voz Alexa", "Sonido mejorado", "Control de dispositivos del hogar", "Stock limitado" }, "/products/echo-dot-5-azul.webp", false, true, "Echo Dot 5.ª generación · Azul", 65m, "SWS-004", "echo-dot-5-azul", 4, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000005"), "3 obsequios", "Apple", new Guid("5a5c0000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Audífonos inalámbricos Apple con estuche de carga inalámbrica. ¡Incluyen 3 obsequios!", new List<string> { "Estuche de carga inalámbrica", "Obsequio: case de silicona", "Obsequio: kit de limpieza", "Obsequio: cable USB-C a USB-C" }, "/products/airpods-5.webp", true, true, "AirPods 5", 250m, "SWS-005", "airpods-5", 5, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000006"), null, "JBL", new Guid("5a5c0000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Audífonos inalámbricos con sonido JBL Pure Bass, sonido espacial y cancelación de ruido adaptable.", new List<string> { "Cancelación de ruido adaptable", "Hasta 48 horas de reproducción", "Resistentes al agua y polvo IP54", "6 micrófonos para llamadas" }, "/products/jbl-tune-flex-2.webp", true, true, "JBL Tune Flex 2", 60m, "SWS-006", "jbl-tune-flex-2", 6, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000007"), null, "Apple", new Guid("5a5c0000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cable original Apple de 1 metro para carga y transferencia de datos de iPhone, iPad y Mac.", new List<string> { "Potencia de hasta 60 W", "Longitud: 1 m", "Conectores USB-C en ambos extremos", "Original Apple" }, "/products/cable-apple-usbc-usbc-60w.webp", false, true, "Cable Apple USB-C a USB-C 60 W", 25m, "SWS-007", "cable-apple-usbc-usbc-60w", 7, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000008"), null, "Apple", new Guid("5a5c0000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cable original Apple de 1 metro para cargar y sincronizar iPhone, iPad y AirPods con conector Lightning.", new List<string> { "Conectores USB-C y Lightning", "Longitud: 1 m", "Compatible con carga rápida", "Original Apple" }, "/products/cable-apple-usbc-lightning.webp", false, true, "Cable Apple USB-C a Lightning", 25m, "SWS-008", "cable-apple-usbc-lightning", 8, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000009"), null, "Apple", new Guid("5a5c0000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Adaptador de corriente original Apple con carga rápida para iPhone, iPad y AirPods.", new List<string> { "Carga rápida de 20 W", "Puerto USB-C", "Compacto y ligero", "Original Apple" }, "/products/cubo-apple-usbc-20w.webp", true, true, "Cubo Apple USB-C 20 W", 25m, "SWS-009", "cubo-apple-usbc-20w", 9, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000010"), null, "Beats", new Guid("5a5c0000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cable trenzado resistente, compatible con iPhone, iPad, Android, parlantes y más.", new List<string> { "Potencia de hasta 60 W", "Compatibilidad universal USB-C", "Diseño resistente", "Original Beats" }, "/products/cable-beats-usbc-60w.webp", false, true, "Cable Beats USB-C a USB-C 60 W", 25m, "SWS-010", "cable-beats-usbc-60w", 10, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000011"), null, null, new Guid("5a5c0000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Funda de silicona para AirPods de 4.ª generación con kit de limpieza y mosquetón incluidos.", new List<string> { "Protección de silicona", "Kit de limpieza incluido", "Mosquetón para llevarlo contigo", "Compacto y dinámico" }, "/products/case-airpods-4-kit-limpieza.webp", false, true, "Case AirPods 4 + Kit de Limpieza", 10m, "SWS-011", "case-airpods-4-kit-limpieza", 11, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000012"), null, null, new Guid("5a5c0000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Carcasa rígida ultradelgada y transparente que se coloca a presión y protege sin ocultar el diseño.", new List<string> { "Ultradelgada y transparente", "Instalación a presión", "Para MacBook Neo de 13 pulgadas", "Stock limitado" }, "/products/carcasa-macbook-neo-13.webp", false, true, "Carcasa MacBook Neo 13\"", 25m, "SWS-012", "carcasa-macbook-neo-13", 12, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000013"), null, "UGREEN", new Guid("5a5c0000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Localizador certificado para Apple Buscar (Find My). Ubica llaves, mochilas o maletas desde tu iPhone.", new List<string> { "Compatible con Apple Buscar", "Funciona con Siri", "Compacto y ligero", "No compatible con Android" }, "/products/ugreen-finetrack.webp", true, true, "UGREEN FineTrack", 15m, "SWS-013", "ugreen-finetrack", 13, 5, null, null },
                    { new Guid("5a5b0000-0000-0000-0000-000000000014"), null, "Puma", new Guid("5a5c0000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Mochila original Puma con amplio compartimento principal y bolsillos frontales con cierre.", new List<string> { "Bolsillos frontales con cierre", "Bolsillos laterales de malla", "Diseño deportivo", "Original Puma" }, "/products/mochila-puma-meridian.webp", false, true, "Mochila Puma Meridian", 40m, "SWS-014", "mochila-puma-meridian", 14, 5, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_Slug",
                table: "Category",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_Number",
                table: "Order",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_PublicToken",
                table: "Order",
                column: "PublicToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_Status_ExpiresAt",
                table: "Order",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductId",
                table: "OrderItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Sku",
                table: "Product",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_Slug",
                table: "Product",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ClientTransactionId",
                table: "Transaction",
                column: "ClientTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_OrderId",
                table: "Transaction",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Category");
        }
    }
}
