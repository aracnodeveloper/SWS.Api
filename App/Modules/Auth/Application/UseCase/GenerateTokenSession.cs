using System.Security.Claims;
using App.Shared.Encrypt;
using App.Shared.Infrastructure;
using App.Shared.JWT;
using Microsoft.Extensions.Options;

namespace App.Modules.Auth;

public class GenerateTokenSession(
    ApplicationContext context,
    PasswordHasher passwordHasher,
    ITokenService tokenService,
    IOptions<TokenOptions> tokenOptions)
{
    public Sesion Login(UserLogin request)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(request.Password))
            throw new Exception("Usuario o contraseña incorrectos.");

        var user = context.User.FirstOrDefault(u => u.Email == email)
            ?? throw new Exception("Usuario o contraseña incorrectos.");

        if (!user.IsActive)
            throw new Exception("La cuenta está deshabilitada.");

        if (string.IsNullOrEmpty(user.Password) || !passwordHasher.Verify(request.Password, user.Password))
            throw new Exception("Usuario o contraseña incorrectos.");

        var roles = context.UserRole
            .Where(ur => ur.UserId == user.Id)
            .Join(context.Role, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Id, r.Name })
            .ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
        };
        // ClaimTypes.Role = nombre del rol (para [Authorize(Roles="...")]).
        // "rid" = id del rol (para SessionAccesor / lógica interna).
        foreach (var r in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, r.Name));
            claims.Add(new Claim("rid", r.Id.ToString()));
        }

        return new Sesion
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = tokenService.GenerateToken(claims),
            ExpiresAt = DateTime.UtcNow.AddMinutes(tokenOptions.Value.ExpireMinutes),
            Roles = roles.Select(r => r.Name).ToList(),
        };
    }
}
