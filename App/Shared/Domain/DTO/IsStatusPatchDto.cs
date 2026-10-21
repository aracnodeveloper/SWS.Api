namespace App.Shared.Domain;

/// <summary>Payload para el PATCH de estado lógico (activar/desactivar).</summary>
public class IsStatusPatchDto
{
    public bool IsStatus { get; set; }
}
