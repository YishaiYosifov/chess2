using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class PoolKeyTests
{
    [Theory]
    [InlineData(PoolType.Casual, 5, 3, "casual:5+3")]
    [InlineData(PoolType.Rated, 10, 0, "rated:10+0")]
    [InlineData(PoolType.Casual, 0, 0, "casual:0+0")]
    [InlineData(PoolType.Rated, 30, 30, "rated:30+30")]
    public void ToGrainKey_returns_the_correct_format(
        PoolType poolType,
        int baseSeconds,
        int incrementSeconds,
        string expectedKey
    )
    {
        TimeControlSettings timeControl = new(baseSeconds, incrementSeconds);
        PoolKey poolKey = new(poolType, timeControl);

        var grainKey = poolKey.ToGrainKey();

        grainKey.Should().Be(expectedKey);
    }

    [Theory]
    [InlineData("casual:5+3", PoolType.Casual, 5, 3)]
    [InlineData("rated:10+0", PoolType.Rated, 10, 0)]
    [InlineData("casual:0+0", PoolType.Casual, 0, 0)]
    [InlineData("rated:30+30", PoolType.Rated, 30, 30)]
    public void FromGrainKey_parses_correctly(
        string grainKey,
        PoolType expectedPoolType,
        int expectedBaseSeconds,
        int expectedIncrementSeconds
    )
    {
        var poolKey = PoolKey.FromGrainKey(grainKey);

        poolKey
            .Should()
            .BeEquivalentTo(
                new PoolKey(
                    expectedPoolType,
                    new TimeControlSettings(expectedBaseSeconds, expectedIncrementSeconds)
                )
            );
    }
}
