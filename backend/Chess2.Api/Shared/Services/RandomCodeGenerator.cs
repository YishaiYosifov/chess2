using System.Security.Cryptography;
using System.Text;

namespace Chess2.Api.Shared.Services;

public interface IRandomCodeGenerator
{
    string GenerateBase62Code(int length);
}

public class RandomCodeGenerator : IRandomCodeGenerator
{
    private const string TokenCharSet =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public string GenerateBase62Code(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            // Map byte to 0-61 range (Base62)
            result.Append(TokenCharSet[b % TokenCharSet.Length]);
        }

        return result.ToString();
    }
}
