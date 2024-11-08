using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Chess2.Api.Services;

public interface IPasswordHasher
{
    Task<byte[]> HashPasswordAsync(string password, byte[] salt);
    byte[] GenerateSalt();
    Task<bool> VerifyPassword(string password, byte[] hash, byte[] salt);
}

public class PasswordHasher : IPasswordHasher
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int DegreeOfParallelism = 4;

    /// <summary>
    /// Generate an argon2id hash for a password with a salt
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <param name="salt">The salt to add to the password to make the has unique</param>
    public Task<byte[]> HashPasswordAsync(string password, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
            Salt = salt,
        };
        return argon2.GetBytesAsync(HashLength);
    }

    /// <summary>
    /// Generate a random salt
    /// </summary>
    public byte[] GenerateSalt()
    {
        var salt = new byte[SaltLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// Verify the password matches the hash and salt
    /// </summary>
    public async Task<bool> VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        var verifyHash = await HashPasswordAsync(password, salt);
        return verifyHash.SequenceEqual(hash);
    }
}
