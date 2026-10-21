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
public class CategoryController(ApplicationContext context, IAppLogger logger)
    : BaseEntityController<CategoryEntity>(context, logger, "Categoría")
{
    /// <summary>Categorías activas con el número de productos activos de cada una.</summary>
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<List<CategoryListDTO>> List() =>
        repository.AsQueryable()
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryListDTO
            {
                Id = c.Id,
                Slug = c.Slug,
                Name = c.Name,
                Description = c.Description,
                SortOrder = c.SortOrder,
                ProductCount = context.Product.Count(p => p.CategoryId == c.Id),
            })
            .ToList();

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public ActionResult<CategoryEntity> GetById(Guid id) => GetEntityById(id);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public ActionResult<CategoryEntity> Create(CategoryEntity entity) => CreateEntity(entity);

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public ActionResult Update(Guid id, CategoryEntity entity) => UpdateEntity(id, entity);

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public ActionResult Delete(Guid id) => DeleteEntity(id);

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public ActionResult PatchStatus(Guid id, [FromBody] IsStatusPatchDto dto) =>
        EntityIsStatusPatch.Execute(context, repository, id, dto);
}
