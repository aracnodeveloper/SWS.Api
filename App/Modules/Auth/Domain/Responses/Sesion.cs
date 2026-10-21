namespace App.Modules.Auth;

public class Sesion
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    /// <summary>Nombres de rol (el front gatea /admin con "Admin").</summary>
    public List<string> Roles { get; set; } = new();
}
