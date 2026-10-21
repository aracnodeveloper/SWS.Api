using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using App.Modules.Auth;
using App.Modules.Catalog;
using App.Modules.Logs;
using App.Modules.Order;
using App.Modules.Payment;
using App.Shared.Application;
using App.Shared.Domain;
using App.Shared.Email;
using App.Shared.Encrypt;
using App.Shared.Infrastructure;
using App.Shared.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace App.Shared.Bootstrap;

public class Application
{
    /// <summary>Política de rate limit para endpoints públicos que reservan stock.</summary>
    public const string CheckoutRateLimit = "checkout";

    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<PasswordHasher>();
        builder.Services.AddScoped<SessionAccesor>();
        builder.Services.AddScoped<IAppLogger, AppLogger>();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

        ConfigureUseCases(builder.Services);
        ConfigureSwagger(builder.Services);
        ConfigureJson(builder.Services);
        ConfigureOptions(builder);
        ConfigureCors(builder);
        ConfigureDatabase(builder);
        ConfigureToken(builder);
        ConfigureAuthorization(builder);
        ConfigureRateLimiting(builder.Services);
        ConfigureCompression(builder.Services);
    }

    /// <summary>Brotli/Gzip para JSON: los listados grandes bajan ~85% de tamaño.</summary>
    private static void ConfigureCompression(IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
    }

    /// <summary>
    /// Checkout reserva stock: se limita por IP para que nadie bloquee el inventario
    /// creando pedidos en bucle.
    /// </summary>
    private static void ConfigureRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(CheckoutRateLimit, http =>
                RateLimitPartition.GetFixedWindowLimiter(
                    http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        });
    }

    /// <summary>
    /// Política de fallback: todo endpoint exige un JWT válido salvo que esté
    /// marcado con [AllowAnonymous]. La gestión de la tienda usa [Authorize(Roles="Admin")].
    /// </summary>
    private static void ConfigureAuthorization(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    public void Configure(WebApplication app)
    {
        ApplyMigrations(app);
        EnsureAdminUser(app);

        app.UseResponseCompression();

        // Imágenes subidas desde el admin: /uploads/** → wwwroot/uploads/**
        // Debe ir antes de auth; si no encuentra el archivo, el request sigue y el fallback JWT da 401.
        var wwwroot = app.Environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(wwwroot))
            wwwroot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        var uploadsRoot = Path.Combine(wwwroot, "uploads");
        Directory.CreateDirectory(Path.Combine(uploadsRoot, "products"));
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsRoot),
            RequestPath = "/uploads",
            ServeUnknownFileTypes = false,
            OnPrepareResponse = ctx =>
                ctx.Context.Response.Headers.CacheControl = "public,max-age=604800",
        });

        app.UseRouting();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("DevelopmentCORSPolicy");
        }
        else
        {
            app.UseCors("ProductionCORSPolicy");
        }

        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }

    private void ConfigureUseCases(IServiceCollection services)
    {
        AuthUseCaseRegister.Register(services);
        CatalogUseCaseRegister.Register(services);
        OrderUseCaseRegister.Register(services);
        PaymentUseCaseRegister.Register(services);
    }

    private static void ConfigureOptions(WebApplicationBuilder builder)
    {
        builder.Services.Configure<AppOptions>(
            builder.Configuration.GetSection(AppOptions.SectionAccesor));
        builder.Services.Configure<PasswordOptions>(
            builder.Configuration.GetSection(PasswordOptions.SectionAccesor));
        builder.Services.Configure<TokenOptions>(
            builder.Configuration.GetSection(TokenOptions.SectionAccesor));
        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.SectionAccesor));
        builder.Services.Configure<StoreOptions>(
            builder.Configuration.GetSection(StoreOptions.SectionAccesor));
        builder.Services.Configure<AdminOptions>(
            builder.Configuration.GetSection(AdminOptions.SectionAccesor));
        builder.Services.Configure<PayPhoneOptions>(
            builder.Configuration.GetSection(PayPhoneOptions.SectionAccesor));
    }

    private static void ConfigureJson(IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                options.Filters.Add(new StandardApiResponseEnvelopeFilter());
            })
            .AddNewtonsoftJson(options =>
            {
                // Sobre { code, message, data } (éxito y error; en errores data es null).
                options.SerializerSettings.MaxDepth = 64;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.ContractResolver =
                    new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new TimeOnlyJsonConverter());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.MaxDepth = 6;
            });
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sale World Sale API",
                Version = "v1",
                Description = "API de catálogo, pedidos y pagos de la tienda SWS."
            });
            options.CustomSchemaIds(type =>
                (type.FullName ?? type.Name).Replace("+", ".", StringComparison.Ordinal));
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Ingrese: `Bearer {token}`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        var appOptions = builder.Configuration
            .GetSection(AppOptions.SectionAccesor)
            .Get<AppOptions>();

        var webOrigins = (appOptions?.WebUrl ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentCORSPolicy", policy => policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader());

            options.AddPolicy("ProductionCORSPolicy", policy =>
            {
                if (webOrigins.Length > 0)
                    policy.WithOrigins(webOrigins);
                else
                    policy.SetIsOriginAllowed(_ => false);
                policy.AllowAnyMethod().AllowAnyHeader();
            });
        });
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        var appOptions = builder.Configuration
            .GetSection(AppOptions.SectionAccesor)
            .Get<AppOptions>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (appOptions?.DatabaseAdapter is null || connectionString is null)
            throw new Exception("Falta el adaptador de base de datos o la cadena de conexión.");

        if (appOptions.DatabaseAdapter != "postgresql")
            throw new Exception("Adaptador de base de datos no soportado.");

        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseNpgsql(connectionString, o => o.CommandTimeout(60));
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
    }

    private static void ConfigureToken(WebApplicationBuilder builder)
    {
        var tokenOptions = builder.Configuration
            .GetSection(TokenOptions.SectionAccesor)
            .Get<TokenOptions>();

        if (tokenOptions is null
            || string.IsNullOrWhiteSpace(tokenOptions.SecretKey)
            || string.IsNullOrWhiteSpace(tokenOptions.Issuer)
            || string.IsNullOrWhiteSpace(tokenOptions.Audience))
        {
            throw new InvalidOperationException(
                "JWT mal configurado: JWT:SecretKey, JWT:Issuer y JWT:Audience deben tener valor.");
        }

        builder.Services.AddSingleton<ITokenService, JWTService>();
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(tokenOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                };
            });
    }

    /// <summary>
    /// Aplica migraciones al arrancar (dev y prod). Reintenta mientras Postgres levanta
    /// en docker compose.
    /// </summary>
    private static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Database");

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                db.Database.Migrate();
                logger.LogInformation("Migraciones EF aplicadas.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning("BD no disponible (intento {Attempt}/{Max}): {Msg}",
                    attempt, maxAttempts, ex.Message);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }

    private static void EnsureAdminUser(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<EnsureAdminUser>().Execute();
    }
}
