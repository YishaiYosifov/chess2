using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public abstract class BasePoolTests<TPool> : BaseUnitTest
    where TPool : IMatchmakingPool
{
    protected abstract TPool Pool { get; }

    protected abstract Seeker AddSeek(string userId);

    [Fact]
    public void AddSeek_adds_the_seeker()
    {
        var seeker = AddSeek("user1");

        Pool.Seekers.Should().ContainSingle().Which.Should().BeEquivalentTo(seeker);
    }

    [Fact]
    public void RemoveSeek_only_removes_the_correct_seeker()
    {
        AddSeek("user1");
        var keepSeeker = AddSeek("user2");

        var result = Pool.RemoveSeek("user1");

        result.Should().BeTrue();
        Pool.Seekers.Should().ContainSingle().Which.Should().Be(keepSeeker);
    }

    [Fact]
    public void HasSeek_returns_true_if_user_has_seek()
    {
        AddSeek("user1");

        Pool.HasSeek("user1").Should().BeTrue();
    }

    [Fact]
    public void HasSeek_returns_false_if_user_does_not_have_seek()
    {
        AddSeek("user1");

        Pool.HasSeek("user2").Should().BeFalse();
    }

    [Fact]
    public void HasSeek_returns_false_after_seek_is_removed()
    {
        AddSeek("user1");
        Pool.RemoveSeek("user1");

        Pool.HasSeek("user1").Should().BeFalse();
    }
}
