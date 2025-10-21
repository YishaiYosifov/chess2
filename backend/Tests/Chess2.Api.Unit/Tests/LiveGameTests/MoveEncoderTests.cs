using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Game.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

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
            new(
                FromIdx: 1,
                ToIdx: 2,
                MoveKey: "move1",
                CapturedIdxs: [4, 5, 6],
                TriggerIdxs: [7, 8, 9],
                SideEffects: [new(FromIdx: 10, ToIdx: 11), new(FromIdx: 12, ToIdx: 13)],
                PieceSpawns:
                [
                    new(Type: PieceType.Pawn, Color: GameColor.White, PosIdx: 14),
                    new(Type: PieceType.Knook, Color: GameColor.Black, PosIdx: 15),
                ],
                IntermediateIdxs: [16, 17, 18],
                PromotesTo: PieceType.Queen
            ),
            new(
                FromIdx: 1,
                ToIdx: 2,
                MoveKey: "move2",
                CapturedIdxs: null,
                TriggerIdxs: null,
                IntermediateIdxs: null,
                SideEffects: null,
                PieceSpawns: null,
                PromotesTo: null
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
