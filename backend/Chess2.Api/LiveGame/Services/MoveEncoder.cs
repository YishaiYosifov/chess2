﻿using System.IO.Compression;
using System.Text.Json;
using Chess2.Api.GameSnapshot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Services;

public interface IMoveEncoder
{
    byte[] EncodeMoves(IEnumerable<MovePath> moves);
}

public class MoveEncoder(IOptions<JsonOptions> jsonOptions) : IMoveEncoder
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;

    public byte[] EncodeMoves(IEnumerable<MovePath> moves)
    {
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            JsonSerializer.Serialize(brotli, moves, _jsonOptions);
        }
        return output.ToArray();
    }
}
