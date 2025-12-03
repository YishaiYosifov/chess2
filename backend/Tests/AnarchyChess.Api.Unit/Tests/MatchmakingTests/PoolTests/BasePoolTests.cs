using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services.Pools;
using AnarchyChess.Api.Profile.Models;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.MatchmakingTests.PoolTests;

public abstract class BasePoolTests<TPool> : BaseUnitTest
    where TPool : IMatchmakingPool
{
    protected abstract TPool Pool { get; }

    protected abstract Seeker AddSeeker(UserId userId);

    [Fact]
    public void AddSeeker_adds_the_seeker()
    {
        var seeker = AddSeeker("user1");

        Pool.Seekers.Should().ContainSingle().Which.Should().BeEquivalentTo(seeker);
    }

    [Fact]
    public void RemoveSeeker_only_removes_the_correct_seeker()
    {
        AddSeeker("user1");
        var keepSeeker = AddSeeker("user2");

        var result = Pool.RemoveSeeker("user1");

        result.Should().BeTrue();
        Pool.Seekers.Should().ContainSingle().Which.Should().Be(keepSeeker);
    }

    [Fact]
    public void HasSeeker_returns_true_if_user_has_seek()
    {
        AddSeeker("user1");

        Pool.HasSeeker("user1").Should().BeTrue();
    }

    [Fact]
    public void HasSeeker_returns_false_if_user_does_not_have_seek()
    {
        AddSeeker("user1");

        Pool.HasSeeker("user2").Should().BeFalse();
    }

    [Fact]
    public void HasSeeker_returns_false_after_seek_is_removed()
    {
        AddSeeker("user1");
        Pool.RemoveSeeker("user1");

        Pool.HasSeeker("user1").Should().BeFalse();
    }

    [Fact]
    public void TryGetSeeker_returns_the_correct_seeker()
    {
        var seeker = AddSeeker("user1");
        AddSeeker("user2");

        Pool.TryGetSeeker(seeker.UserId, out var foundSeeker).Should().BeTrue();
        foundSeeker.Should().Be(seeker);
    }

    [Fact]
    public void TryGetSeeker_returns_null_when_the_seeker_doesnt_exist()
    {
        AddSeeker("user1");

        Pool.TryGetSeeker("random user id", out var foundSeeker).Should().BeFalse();
        foundSeeker.Should().BeNull();
    }
}
