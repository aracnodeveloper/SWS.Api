using System.Net;
using App.Modules.Logs;
using App.Shared;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Catalog;

[ApiController]
[Route("api/[controller]")]
public class ProductController(
    ApplicationContext context,
    IAppLogger logger,
    UploadProductImage uploadProductImage)
    : BaseEntityController<ProductEntity>(context, logger, "Producto")
{
    /// <summary>
    /// Catálogo público (solo activos). Filtros: <paramref name="category"/> (slug),
    /// <paramref name="search"/> (nombre/marca), <paramref name="featured"/>.
    /// Orden: <c>relevance</c> (SortOrder) | <c>price_asc</c> | <c>price_desc</c> | <c>newest</c>.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? featured = null,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24)
    {
        var query = repository.AsQueryable().AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category != null && p.Category.Slug == category);
        if (featured == true)
            query = query.Where(p => p.IsFeatured);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, term) || (p.Brand != null && EF.Functions.ILike(p.Brand, term)));
        }

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.SortOrder),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.SortOrder),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.SortOrder),
        };

        return await ListEntities(query, page, Math.Clamp(pageSize, 1, 100));
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public ActionResult<ProductEntity> GetBySlug(string slug)
    {
        var product = repository.AsQueryable()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Slug == slug);
        return product is null
            ? ErrorMessage("Producto no existe", HttpStatusCode.NotFound)
            : Ok(product);
    }

    /// <summary>Listado admin: incluye inactivos.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult> AdminList(
        [FromQuery] string? search = null,
        [FromQuery] bool? isStatus = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = repository.IgnoreQueryFilters().AsNoTracking().Include(p => p.Category).AsQueryable();
        if (isStatus is bool s)
            query = query.Where(p => p.IsStatus == s);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term) || EF.Functions.ILike(p.Sku, term));
        }
        return await ListEntities(query.OrderBy(p => p.SortOrder), page, pageSize);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public ActionResult<ProductEntity> GetById(Guid id) => GetEntityById(id);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public ActionResult<ProductEntity> Create(ProductEntity entity)
    {
        entity.Category = null;
        return CreateEntity(entity);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public ActionResult Update(Guid id, ProductEntity entity)
    {
        entity.Category = null;
        return UpdateEntity(id, entity);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public ActionResult Delete(Guid id) => DeleteEntity(id);

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public ActionResult PatchStatus(Guid id, [FromBody] IsStatusPatchDto dto) =>
        EntityIsStatusPatch.Execute(context, repository, id, dto);

    /// <summary>Ajuste rápido de inventario desde el admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/stock")]
    public ActionResult PatchStock(Guid id, [FromBody] StockPatchDto dto)
    {
        if (dto.Stock < 0)
            return ErrorMessage("El stock no puede ser negativo", HttpStatusCode.BadRequest);

        var product = repository.IgnoreQueryFilters().FirstOrDefault(p => p.Id == id);
        if (product is null)
            return ErrorMessage("Producto no existe", HttpStatusCode.NotFound);

        product.Stock = dto.Stock;
        context.SaveChanges();
        _logger.Log($"Stock de {product.Sku} ajustado a {dto.Stock}.", LogType.SUCCESS);
        return Ok(new HttpMessage("Stock actualizado"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        try
        {
            return Ok(await uploadProductImage.ExecuteAsync(id, file, ct));
        }
        catch (BadRequestException e)
        {
            return ErrorMessage(e.Message, HttpStatusCode.BadRequest);
        }
    }
}
