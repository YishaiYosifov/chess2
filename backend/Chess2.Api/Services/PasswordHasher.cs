using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Chess2.Api.Services;

public class PasswordHasher
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int DegreeOfParallelism = 4;

    public byte[] HashPassword(string password, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
            Salt = salt,
        };
        return argon2.GetBytes(HashLength);
    }

    public byte[] GenerateSalt()
    {
        var salt = new byte[SaltLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}
