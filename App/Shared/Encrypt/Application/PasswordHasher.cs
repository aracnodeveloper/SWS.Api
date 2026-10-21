using System.Security.Cryptography;
using System.Text;
using App.Shared.Encrypt;
using Microsoft.Extensions.Options;

namespace App.Shared.Encrypt;

/// <summary>
/// Hash de contraseñas con salt + pepper (PBKDF2). Nunca almacena texto plano.
/// </summary>
public class PasswordHasher
{
    private readonly PasswordOptions _options;
    private const int Iterations = 100_000;
    private const int HashBytes = 32;

    public PasswordHasher(IOptions<PasswordOptions> options)
    {
        _options = options.Value;
    }

    public string ComputeHash(string password)
    {
        var salted = _options.Salt + password + _options.Pepper;
        using var pbkdf2 = new Rfc2898DeriveBytes(
            salted,
            Encoding.UTF8.GetBytes(_options.Salt),
            Iterations,
            HashAlgorithmName.SHA256
        );
        return Convert.ToBase64String(pbkdf2.GetBytes(HashBytes));
    }

    public bool Verify(string password, string hash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(ComputeHash(password)),
            Encoding.UTF8.GetBytes(hash)
        );


    public string GetRandomHash() => Guid.NewGuid().ToString("N")[..10];
}
