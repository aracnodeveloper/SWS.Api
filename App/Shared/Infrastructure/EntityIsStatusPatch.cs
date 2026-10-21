using App.Shared.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Shared.Infrastructure;

/// <summary>Activa/desactiva (borrado lógico) una entidad <see cref="IEntityStatus"/>.</summary>
public static class EntityIsStatusPatch
{
    public static ActionResult Execute<T>(
        ApplicationContext context,
        DbSet<T> repository,
        Guid id,
        IsStatusPatchDto dto
    )
        where T : class, IEntity, IEntityStatus
    {
        var entity = repository.IgnoreQueryFilters().FirstOrDefault(e => e.Id == id);
        if (entity == null)
            return new NotFoundObjectResult(new { message = "Registro no encontrado" });

        entity.IsStatus = dto.IsStatus;
        context.SaveChanges();

        // No devolver la entidad: un soft-delete no debe “aparecer” otra vez en data.
        return new OkObjectResult(new HttpMessage(
            dto.IsStatus ? "Registro activado" : "Registro desactivado"));
    }
}
