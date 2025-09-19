using System.IO.Compression;
using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class MoveEncoderTests
{
    private readonly MoveEncoder _encoder;

    public MoveEncoderTests()
    {
        var jsonOptions = new MvcNewtonsoftJsonOptions();
        jsonOptions.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();
        jsonOptions.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

        _encoder = new MoveEncoder(Options.Create(jsonOptions));
    }

    [Fact]
    public void EncodeMoves_round_trips_encoding_losslessly()
    {
        MovePath[] paths =
        [
            new(
                FromIdx: 1,
                ToIdx: 2,
                CapturedIdxs: [4, 5, 6],
                TriggerIdxs: [7, 8, 9],
                SideEffects: [new(FromIdx: 10, ToIdx: 11), new(FromIdx: 12, ToIdx: 13)],
                IntermediateIdxs: [14, 15, 16],
                PromotesTo: PieceType.Queen
            ),
            new(
                FromIdx: 1,
                ToIdx: 2,
                CapturedIdxs: null,
                TriggerIdxs: null,
                IntermediateIdxs: null,
                SideEffects: null,
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

    private static List<MovePath>? DecompressToPath(byte[] gzipBytes)
    {
        using var input = new MemoryStream(gzipBytes);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(brotli, Encoding.UTF8);
        var paths = JsonConvert.DeserializeObject<List<MovePath>>(reader.ReadToEnd());
        return paths;
    }
}
