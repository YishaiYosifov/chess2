using System.IO.Compression;
using Chess2.Api.GameSnapshot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Chess2.Api.LiveGame.Services;

public interface IMoveEncoder
{
    byte[] EncodeMoves(IEnumerable<MovePath> moves);
}

public class MoveEncoder(IOptions<MvcNewtonsoftJsonOptions> jsonOptions) : IMoveEncoder
{
    private readonly MvcNewtonsoftJsonOptions _jsonOptions = jsonOptions.Value;

    public byte[] EncodeMoves(IEnumerable<MovePath> moves)
    {
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true))
        using (var streamWriter = new StreamWriter(brotli))
        using (var jsonWriter = new JsonTextWriter(streamWriter))
        {
            var serializer = JsonSerializer.Create(_jsonOptions.SerializerSettings);
            serializer.Serialize(jsonWriter, moves);
        }
        return output.ToArray();
    }
}
