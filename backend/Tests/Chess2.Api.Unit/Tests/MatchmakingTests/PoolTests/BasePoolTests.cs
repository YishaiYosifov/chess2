using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public abstract class BasePoolTests<TPool> : BaseUnitTest
    where TPool : IMatchmakingPool
{
    protected abstract TPool Pool { get; }

    protected abstract void AddSeek(string userId);

    [Fact]
    public void AddSeek_adds_the_seeker()
    {
        AddSeek("user1");

        Pool.Seekers.Should().ContainSingle().Which.Should().BeEquivalentTo("user1");
    }

    [Fact]
    public void RemoveSeek_only_removes_the_correct_seeker()
    {
        AddSeek("user1");
        AddSeek("user2");

        var result = Pool.RemoveSeek("user1");

        result.Should().BeTrue();
        Pool.Seekers.Should().ContainSingle().Which.Should().Be("user2");
    }
}
