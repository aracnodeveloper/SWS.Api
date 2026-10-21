namespace App.Shared;

/// <summary>
/// Estado lógico del registro. IsStatus true = activo (listados/paginación);
/// false tras borrado lógico. Las entidades con borrado físico no implementan esta interfaz.
/// </summary>
public interface IEntityStatus
{
    bool IsStatus { get; set; }
}
