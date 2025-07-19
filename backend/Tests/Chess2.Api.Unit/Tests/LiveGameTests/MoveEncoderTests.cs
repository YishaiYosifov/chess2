using System.IO.Compression;
using System.Text;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using FluentAssertions;
using Newtonsoft.Json;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class MoveEncoderTests : BaseUnitTest
{
    private readonly MoveEncoder _encoder = new();

    [Fact]
    public void EncodeMoves_RoundTripsObjectsLosslessly()
    {
        MovePath[] paths =
        [
            new(
                FromIdx: 1,
                ToIdx: 2,
                CapturedIdxs: [4, 5, 6],
                TriggerIdxs: [7, 8, 9],
                SideEffects: [new(FromIdx: 10, ToIdx: 11), new(FromIdx: 12, ToIdx: 13)]
            ),
            new(FromIdx: 1, ToIdx: 2, CapturedIdxs: null, TriggerIdxs: null, SideEffects: null),
        ];

        var result = _encoder.EncodeMoves(paths);

        var decompressed = DecompressToPath(result);
        decompressed.Should().BeEquivalentTo(paths);
    }

    [Fact]
    public void EncodeMoves_WithEmptyCollection_ReturnsGzipOfEmptyArray()
    {
        var result = _encoder.EncodeMoves([]);

        var decompressed = DecompressToPath(result);

        decompressed.Should().BeEmpty();
    }

    private static List<MovePath>? DecompressToPath(byte[] gzipBytes)
    {
        using var input = new MemoryStream(gzipBytes);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        var paths = JsonConvert.DeserializeObject<List<MovePath>>(reader.ReadToEnd());
        return paths;
    }
}
