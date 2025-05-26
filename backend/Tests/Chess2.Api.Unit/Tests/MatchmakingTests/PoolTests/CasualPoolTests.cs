using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class CasualPoolTests : BasePoolTests<CasualMatchmakingPool>
{
    protected override CasualMatchmakingPool Pool { get; } = new();

    protected override void AddSeek(string userId) => Pool.AddSeek(userId);

    [Fact]
    public void CalculateMatches_returns_empty_list_when_no_seekers()
    {
        var matches = Pool.CalculateMatches();
        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_returns_all_pairs_with_even_number_of_seekers()
    {
        Pool.AddSeek("User1");
        Pool.AddSeek("User2");
        Pool.AddSeek("User3");
        Pool.AddSeek("User4");

        var matches = Pool.CalculateMatches();

        matches.Should().HaveCount(2);
        matches.Should().Contain(("User1", "User2"));
        matches.Should().Contain(("User3", "User4"));

        Pool.Seekers.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_leaves_last_seeker_unmatched_with_odd_number_of_seekers()
    {
        Pool.AddSeek("User1");
        Pool.AddSeek("User2");
        Pool.AddSeek("User3");

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().Be(("User1", "User2"));

        Pool.Seekers.Should().ContainSingle().Which.Should().Be("User3");
    }
}
