using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class MoveEncoderTests
{
    private readonly MoveEncoder _encoder;
    private readonly JsonSerializerOptions _jsonOptions;

    public MoveEncoderTests()
    {
        var jsonOptions = new JsonOptions();
        jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;

        _encoder = new MoveEncoder(Options.Create(jsonOptions));
        _jsonOptions = jsonOptions.JsonSerializerOptions;
    }

    [Fact]
    public void EncodeMoves_round_trips_encoding_losslessly()
    {
        MovePath[] paths =
        [
            new MovePathFaker().Generate(),
            new(
                FromIdx: 1,
                ToIdx: 2,
                MoveKey: "move2",
                CapturedIdxs: null,
                TriggerIdxs: null,
                IntermediateSquares: null,
                SideEffects: null,
                PieceSpawns: null,
                PromotesTo: null,
                SpecialMoveType: null
            ),
        ];

        var result = _encoder.EncodeMoves(paths);

        var decompressed = DecompressToPath(result);
        decompressed.Should().BeEquivalentTo(paths);
    }

    [Fact]
    public void EncodeMoves_can_handle_empty_move_collection()
    {
        var result = _encoder.EncodeMoves([]);

        var decompressed = DecompressToPath(result);

        decompressed.Should().BeEmpty();
    }

    private List<MovePath>? DecompressToPath(byte[] compressedBytes)
    {
        using var input = new MemoryStream(compressedBytes);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(brotli, Encoding.UTF8);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<List<MovePath>>(json, _jsonOptions);
    }
}
