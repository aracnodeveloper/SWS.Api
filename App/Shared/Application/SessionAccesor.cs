using System.Security.Claims;
using App.Shared.JWT;

namespace App.Shared.Application;

public class SessionAccesor
{
    public Guid? UserId { get; internal set; }
    public List<Guid> RolesId { get; internal set; } = new();
    public string? IP { get; internal set; }
    public bool IsHttpRequest { get; internal set; }
    public bool IsAuditableEnabled { get; internal set; } = true;

    /// <summary>Solo para herramientas de diseño (dotnet ef migrations): sin HTTP ni JWT.</summary>
    public SessionAccesor()
    {
        IsHttpRequest = false;
    }

    public SessionAccesor(IHttpContextAccessor accesor, ITokenService tokenService)
    {
        IsHttpRequest = accesor.HttpContext?.Request is not null;
        if (!IsHttpRequest)
            return;

        var token = accesor
            .HttpContext?.Request.Headers["Authorization"]
            .FirstOrDefault()
            ?.Replace("Bearer ", "");

        if (token is not null && tokenService.IsTokenValid(token))
        {
            var userId = tokenService.GetClaimValue(token, ClaimTypes.Sid);
            var claims = tokenService.GetClaims(token);

            UserId = userId is not null ? Guid.Parse(userId) : Guid.Empty;

            // "rid" transporta el id del rol; ClaimTypes.Role transporta el nombre.
            RolesId = claims
                .Where(c => c.Type == "rid")
                .Select(c => Guid.Parse(c.Value))
                .ToList();
        }

        IP = accesor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
    }

    internal void SetAuditable(bool estado) => IsAuditableEnabled = estado;
}
