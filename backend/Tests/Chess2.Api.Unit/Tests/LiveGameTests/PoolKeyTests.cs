using System.Text.Json;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class PoolKeyTests
{
    [Fact]
    public void ToString_serializes_correctly()
    {
        var key = new PoolKey(PoolType.Casual, new TimeControlSettings(300, 5));
        var result = key.ToString();

        var expectedJson = JsonSerializer.Serialize(key);
        result.Should().Be(expectedJson);
    }

    [Fact]
    public void Parse_deserializes_correctly()
    {
        var original = new PoolKey(PoolType.Rated, new TimeControlSettings(600, 10));
        var json = original.ToString();

        var parsed = PoolKey.Parse(json);

        parsed.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Parse_throws_when_provided_with_null_json()
    {
        var act = () => PoolKey.Parse("null");
        act.Should().Throw<FormatException>().WithMessage("Invalid PoolKey JSON");
    }
}
