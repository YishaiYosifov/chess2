using AutoFixture;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class RatedPoolTests : BasePoolTests<RatedMatchmakingPool>
{
    private readonly GameSettings _settings;
    protected override RatedMatchmakingPool Pool { get; }

    public RatedPoolTests()
    {
        var settings = Fixture.Create<IOptions<AppSettings>>();
        _settings = settings.Value.Game;
        Pool = new RatedMatchmakingPool(settings);
    }

    protected override void AddSeek(string userId) => Pool.AddSeek(userId, 1200);

    [Fact]
    public void CalculateMatches_matches_within_range()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1200 + _settings.StartingMatchRatingDifference);

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_does_not_match_outside_range()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1200 + _settings.StartingMatchRatingDifference + 1);

        var matches = Pool.CalculateMatches();

        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_increases_rating_range_after_waves_missed()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek(
            "user2",
            1200
                + _settings.StartingMatchRatingDifference
                + _settings.MatchRatingDifferenceGrowthPerWave
        );
        // First wave: no match
        var matches1 = Pool.CalculateMatches();
        matches1.Should().BeEmpty();

        // Second wave: range grows, now matches
        var matches2 = Pool.CalculateMatches();
        matches2.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_prioritizes_old_seeks()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1210);
        Pool.AddSeek("user3", 1215);

        // match user 1 and user 2
        var matches1 = Pool.CalculateMatches();
        matches1.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));

        // re-add the users
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1210);
        // now user 3 has missed a wave, so it should be prioritized
        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user3", "user2"));
    }

    [Fact]
    public void CalculateMatches_no_seekers_returns_empty()
    {
        var matches = Pool.CalculateMatches();
        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_single_seeker_returns_no_matches()
    {
        Pool.AddSeek("user1", 1500);
        var matches = Pool.CalculateMatches();
        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_match_prefers_closer_rating_when_waves_missed_equal()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1210);
        Pool.AddSeek("user3", 1215);

        // Both user2 and user3 can match with user1, but user2 has closer rating difference
        var matches = Pool.CalculateMatches();
        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_handles_multiple_pairs()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1205);
        Pool.AddSeek("user3", 1210);
        Pool.AddSeek("user4", 1195);

        var matches = Pool.CalculateMatches();

        // Should create two matches
        matches.Count.Should().Be(2);

        var allMatchedUsers = matches.SelectMany(m => new[] { m.userId1, m.userId2 }).ToList();
        allMatchedUsers.Should().Contain(["user1", "user2", "user3", "user4"]);
    }

    [Fact]
    public void CalculateMatches_removes_matched_users()
    {
        Pool.AddSeek("user1", 1200);
        Pool.AddSeek("user2", 1205);
        Pool.AddSeek("user3", 1205);

        Pool.SeekerCount.Should().Be(3);

        var matches1 = Pool.CalculateMatches();
        matches1.Should().ContainSingle();

        Pool.Seekers.Should().ContainSingle().Which.Should().Be("user3");

        var matches2 = Pool.CalculateMatches();
        matches2.Should().BeEmpty();
    }
}
