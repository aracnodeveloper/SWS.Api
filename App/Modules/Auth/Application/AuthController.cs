using App.Modules.Logs;
using App.Shared.Application;
using App.Shared.Bootstrap;
using App.Shared.Domain;
using App.Shared.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace App.Modules.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    GenerateTokenSession generateTokenSession,
    ApplicationContext context,
    SessionAccesor session,
    IAppLogger logger) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting(Application.CheckoutRateLimit)]
    [HttpPost("login")]
    public ActionResult<Sesion> Login(UserLogin request)
    {
        try
        {
            return Ok(generateTokenSession.Login(request));
        }
        catch (Exception e)
        {
            logger.Log($"[Auth] Login fallido para {request.Email}: {e.Message}", LogType.WARNING);
            return BadRequest(new HttpMessage(e.Message));
        }
    }

    /// <summary>Datos del usuario autenticado (valida que el token siga vigente).</summary>
    [HttpGet("me")]
    public ActionResult Me()
    {
        var user = session.UserId is Guid id ? context.User.Find(id) : null;
        if (user is null || !user.IsActive)
            return Unauthorized(new HttpMessage("Sesión no válida."));

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            Roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList(),
        });
    }
}
