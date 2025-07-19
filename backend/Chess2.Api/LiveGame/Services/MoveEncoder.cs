using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IMoveEncoder
{
    byte[] EncodeMoves(IEnumerable<MovePath> moves);
}

public class MoveEncoder : IMoveEncoder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public byte[] EncodeMoves(IEnumerable<MovePath> moves)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
        {
            JsonSerializer.Serialize(gzip, moves, JsonOptions);
        }
        return output.ToArray();
    }
}
