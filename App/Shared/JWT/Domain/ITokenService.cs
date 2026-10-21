using System.Security.Claims;

namespace App.Shared.JWT;

public interface ITokenService
{
    string GenerateToken(IEnumerable<Claim> claims);
    bool IsTokenValid(string token);
    IEnumerable<Claim> GetClaims(string token);
    string? GetClaimValue(string token, string claimType);
}
