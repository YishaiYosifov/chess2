using System.Security.Cryptography;
using System.Text;

namespace Chess2.Api.Shared.Services;

public interface IRandomCodeGenerator
{
    string Generate(int length);
}

public class RandomCodeGenerator : IRandomCodeGenerator
{
    private const string TokenCharSet =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public string Generate(int length)
    {
        StringBuilder result = new(length);
        for (int i = 0; i < length; i++)
        {
            int index = RandomNumberGenerator.GetInt32(TokenCharSet.Length);
            result.Append(TokenCharSet[index]);
        }

        return result.ToString();
    }
}
