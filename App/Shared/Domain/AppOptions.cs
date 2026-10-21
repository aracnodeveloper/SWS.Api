namespace App.Shared.Domain;

public class AppOptions
{
    public const string SectionAccesor = "AppOptions";
    public required string AppName { get; set; }
    public required string DatabaseAdapter { get; set; }
    public required string ApiUrl { get; set; }
    public required string WebUrl { get; set; }
    /// <summary>Origen público de la tienda (links de retorno de pagos y correos), p. ej. http://localhost:4200.</summary>
    public string? PublicWebUrl { get; set; }
    public int MaxRequestSizeMB { get; set; } = 50;
}
