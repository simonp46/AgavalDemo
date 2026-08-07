using System.Globalization;
using System.Security.Cryptography;
using GestorInventario.Application.Interfaces;

namespace GestorInventario.Infrastructure.Security;

internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA512";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return string.Join(
            '$',
            Algorithm,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string passwordHash, string password)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || password is null)
        {
            return false;
        }

        var parts = passwordHash.Split('$');

        if (parts.Length != 4 ||
            !string.Equals(parts[0], Algorithm, StringComparison.Ordinal) ||
            !int.TryParse(
                parts[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var iterations) ||
            iterations is < 100_000 or > 1_000_000)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);

            if (salt.Length < SaltSize || expectedHash.Length != HashSize)
            {
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
