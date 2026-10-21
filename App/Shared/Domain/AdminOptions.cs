namespace App.Shared.Domain;

/// <summary>Usuario administrador inicial. Sección <c>Admin</c>.</summary>
public class AdminOptions
{
    public const string SectionAccesor = "Admin";

    public string Email { get; set; } = "";
    public string Name { get; set; } = "Administrador";

    /// <summary>Solo se usa para crear el primer usuario; vacío = no se crea.</summary>
    public string Password { get; set; } = "";
}
