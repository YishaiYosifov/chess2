using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public abstract class BasePoolTests<TPool> : BaseUnitTest
    where TPool : IMatchmakingPool
{
    protected abstract TPool Pool { get; }

    [Fact]
    public void AddSeek_adds_the_seeker()
    {
        Pool.AddSeek("user1", 1200);

        var expectedSeek = new SeekInfo("user1", 1200);
        Pool.Seekers.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedSeek);
    }

    [Fact]
    public void RemoveSeek_only_removes_the_correct_seeker()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1200);

        var result = Pool.RemoveSeek("user1");

        result.Should().BeTrue();
        Pool.Seekers.Should().ContainSingle().Which.UserId.Should().Be("user2");
    }
}
