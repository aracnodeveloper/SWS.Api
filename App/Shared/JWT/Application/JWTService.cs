using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Shared.JWT;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.Shared.JWT;

public class JWTService : ITokenService
{
    private readonly TokenOptions _options;

    public JWTService(IOptions<TokenOptions> options)
    {
        _options = options.Value;
    }

    private SymmetricSecurityKey Key =>
        new(Encoding.UTF8.GetBytes(_options.SecretKey));

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenValidationParameters ValidationParameters => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = Key,
        ClockSkew = TimeSpan.Zero
    };

    public bool IsTokenValid(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<Claim> GetClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return Enumerable.Empty<Claim>();
        return handler.ReadJwtToken(token).Claims;
    }

    public string? GetClaimValue(string token, string claimType) =>
        GetClaims(token).FirstOrDefault(c => c.Type == claimType)?.Value;
}
