using App.Shared.Domain;
using App.Shared.Infrastructure;

namespace App.Modules.Catalog;

/// <summary>Guarda la imagen principal de un producto en wwwroot/uploads/products.</summary>
public class UploadProductImage(ApplicationContext context, IWebHostEnvironment env)
{
    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, byte[][]> Signatures = new()
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]], // "RIFF"
    };

    public async Task<ProductEntity> ExecuteAsync(Guid productId, IFormFile file, CancellationToken ct)
    {
        var product = context.Product.FirstOrDefault(p => p.Id == productId)
            ?? throw new BadRequestException("Producto no existe.");

        if (file.Length == 0 || file.Length > MaxBytes)
            throw new BadRequestException("La imagen debe pesar entre 1 byte y 5 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Signatures.TryGetValue(ext, out var signatures))
            throw new BadRequestException("Formato no permitido (jpg, png o webp).");

        // Valida la firma real del archivo, no solo la extensión.
        var header = new byte[4];
        await using (var peek = file.OpenReadStream())
            _ = await peek.ReadAsync(header, ct);
        if (!signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig)))
            throw new BadRequestException("El archivo no es una imagen válida.");

        var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;
        var folder = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(folder);

        var fileName = $"{product.Slug[..Math.Min(40, product.Slug.Length)]}-{Guid.NewGuid():N}{ext}";
        await using (var stream = File.Create(Path.Combine(folder, fileName)))
            await file.CopyToAsync(stream, ct);

        DeletePreviousUpload(webRoot, product.ImageUrl);
        product.ImageUrl = $"/uploads/products/{fileName}";
        await context.SaveChangesAsync(ct);
        return product;
    }

    /// <summary>Solo borra archivos subidos al API (nunca assets del front).</summary>
    private static void DeletePreviousUpload(string webRoot, string? imageUrl)
    {
        if (imageUrl is null || !imageUrl.StartsWith("/uploads/products/", StringComparison.Ordinal))
            return;
        var path = Path.Combine(webRoot, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path))
            File.Delete(path);
    }
}
