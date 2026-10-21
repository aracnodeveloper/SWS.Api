using System.Net;
using App.Modules.Logs;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using App.Shared.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Shared;

public class BaseEntityController<T> : Controller
    where T : class, IEntity
{
    protected ApplicationContext context;
    protected DbSet<T> repository;
    protected IAppLogger _logger;
    protected string _nombreEntidad;

    public BaseEntityController(ApplicationContext context, IAppLogger logger, string nombreEntidad)
    {
        this.context = context;
        repository = this.context.Set<T>();
        _logger = logger;
        _nombreEntidad = nombreEntidad;
    }

    protected virtual async Task<ActionResult> ListEntities<K>(
        IQueryable<K> results,
        int page,
        int pageSize,
        bool allPages = false
    )
        where K : class
    {
        var pagedResults = await PagedList<K>.CreateAsync(results, page, pageSize, allPages);
        return Ok(pagedResults);
    }

    protected virtual ActionResult GetEntityById(Guid id)
    {
        var result = FindActive(id);
        if (result == null)
        {
            _logger.Log($"{_nombreEntidad} no encontrado ({id})", LogType.ERROR);
            return ErrorMessage($"{_nombreEntidad} no existe", HttpStatusCode.NotFound);
        }
        return Ok(result);
    }

    protected virtual ActionResult CreateEntity(T entity)
    {
        try
        {
            if (entity is IEntityStatus statusEntity)
                statusEntity.IsStatus = true;

            repository.Add(entity);
            context.SaveChanges();
            _logger.Log($"{_nombreEntidad} creado correctamente: {entity.Id}.", LogType.SUCCESS);
            return StatusCode((int)HttpStatusCode.Created, entity);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error al crear {_nombreEntidad}: {ex.Message}", LogType.ERROR);
            return ErrorMessage($"Error al crear {_nombreEntidad.ToLowerInvariant()}");
        }
    }

    protected virtual ActionResult UpdateEntity(Guid id, T entity)
    {
        try
        {
            if (id != entity.Id)
                return ErrorMessage("El id no coincide", HttpStatusCode.BadRequest);

            var result = FindActive(id);
            if (result == null)
                return ErrorMessage($"{_nombreEntidad} no existe", HttpStatusCode.NotFound);

            // Preservar IsStatus: el body del PUT no debe reactivar ni borrar lógicamente.
            bool? previousIsStatus = result is IEntityStatus s ? s.IsStatus : null;

            context.Entry(result).CurrentValues.SetValues(entity);

            if (result is IEntityStatus statusEntity && previousIsStatus.HasValue)
            {
                statusEntity.IsStatus = previousIsStatus.Value;
                context.Entry(result).Property(nameof(IEntityStatus.IsStatus)).IsModified = false;
            }

            context.SaveChanges();
            _logger.Log($"{_nombreEntidad} {id} actualizado.", LogType.SUCCESS);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error al actualizar {_nombreEntidad} {id}: {ex.Message}", LogType.ERROR);
            return ErrorMessage($"Error al actualizar {_nombreEntidad.ToLowerInvariant()}");
        }
    }

    protected virtual ActionResult UpdatePartialEntity(Guid id, JsonPatchDocument<T> patch)
    {
        try
        {
            var result = FindActive(id);
            if (result == null)
                return ErrorMessage($"{_nombreEntidad} no existe", HttpStatusCode.NotFound);

            bool? previousIsStatus = result is IEntityStatus s ? s.IsStatus : null;

            patch.ApplyTo(result);
            if (!ModelState.IsValid)
                return ErrorMessage("Datos de actualización inválidos", HttpStatusCode.BadRequest);

            if (result is IEntityStatus statusEntity && previousIsStatus.HasValue)
            {
                statusEntity.IsStatus = previousIsStatus.Value;
                context.Entry(result).Property(nameof(IEntityStatus.IsStatus)).IsModified = false;
            }

            repository.Entry(result).State = EntityState.Modified;
            context.SaveChanges();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error al parchear {_nombreEntidad} {id}: {ex.Message}", LogType.ERROR);
            return ErrorMessage($"Error al actualizar {_nombreEntidad.ToLowerInvariant()}");
        }
    }

    protected virtual ActionResult DeleteEntity(Guid id)
    {
        try
        {
            var result = typeof(IEntityStatus).IsAssignableFrom(typeof(T))
                ? repository.IgnoreQueryFilters().FirstOrDefault(e => e.Id == id)
                : repository.Find(id);

            if (result == null)
                return ErrorMessage($"{_nombreEntidad} no existe", HttpStatusCode.NotFound);

            if (result is IEntityStatus alreadyDeleted && !alreadyDeleted.IsStatus)
                return ErrorMessage($"{_nombreEntidad} ya está eliminado", HttpStatusCode.BadRequest);

            if (result is IEntityStatus)
            {
                context.Entry(result).Property(nameof(IEntityStatus.IsStatus)).CurrentValue = false;
                context.Entry(result).Property(nameof(IEntityStatus.IsStatus)).IsModified = true;
                context.SaveChanges();
            }
            else
            {
                repository.Remove(result);
                context.SaveChanges();
            }

            _logger.Log($"{_nombreEntidad} {id} eliminado (soft-delete).", LogType.SUCCESS);
            return Ok(new HttpMessage($"{_nombreEntidad} eliminado"));
        }
        catch (Exception ex)
        {
            _logger.Log($"Error al eliminar {_nombreEntidad} {id}: {ex.Message}", LogType.ERROR);
            return ErrorMessage($"Error al eliminar {_nombreEntidad.ToLowerInvariant()}");
        }
    }

    /// <summary>Busca por id solo si está activo (soft-delete → null).</summary>
    protected T? FindActive(Guid id)
    {
        if (!typeof(IEntityStatus).IsAssignableFrom(typeof(T)))
            return repository.Find(id);

        return repository
            .IgnoreQueryFilters()
            .FirstOrDefault(e => e.Id == id && EF.Property<bool>(e, nameof(IEntityStatus.IsStatus)));
    }

    protected ActionResult ErrorMessage(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new ObjectResult(new { message }) { StatusCode = (int)statusCode };
    }
}
