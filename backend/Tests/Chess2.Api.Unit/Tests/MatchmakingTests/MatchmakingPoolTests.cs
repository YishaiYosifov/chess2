using AutoFixture;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class MatchmakingPoolTests : BaseUnitTest
{
    private readonly GameSettings _settings;
    private readonly MatchmakingPool _pool;

    public MatchmakingPoolTests()
    {
        var settings = Fixture.Create<IOptions<AppSettings>>();
        _settings = settings.Value.Game;
        _pool = new MatchmakingPool(settings);
    }

    [Fact]
    public void AddSeek_adds_the_seeker()
    {
        _pool.AddSeek("user1", 1200);

        var expectedSeek = new SeekInfo("user1", 1200);
        _pool.Seekers.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedSeek);
    }

    [Fact]
    public void RemoveSeek_only_removes_the_correct_seeker()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek("user2", 1200);

        var result = _pool.RemoveSeek("user1");

        result.Should().BeTrue();
        _pool.Seekers.Should().ContainSingle().Which.UserId.Should().Be("user2");
    }

    [Fact]
    public void CalculateMatches_matches_within_range()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek("user2", 1200 + _settings.StartingMatchRatingDifference);

        var matches = _pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_does_not_match_outside_range()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek("user2", 1200 + _settings.StartingMatchRatingDifference + 1);

        var matches = _pool.CalculateMatches();

        matches.Should().BeEmpty();
        _pool.Seekers[0].WavesMissed.Should().Be(1);
        _pool.Seekers[1].WavesMissed.Should().Be(1);
    }

    [Fact]
    public void CalculateMatches_matches_after_waves_missed()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek(
            "user2",
            1200
                + _settings.StartingMatchRatingDifference
                + _settings.MatchRatingDifferenceGrowthPerWave
        );
        // First wave: no match
        var matches1 = _pool.CalculateMatches();
        matches1.Should().BeEmpty();

        // Second wave: range grows, now matches
        var matches2 = _pool.CalculateMatches();
        matches2.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_prioritizes_old_seeks()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek("user2", 1210);
        _pool.AddSeek("user3", 1215);

        // match user 1 and user 2
        _pool.CalculateMatches();
        // now user 3 has missed a wave, so it should be prioritized
        var matches = _pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user3", "user2"));
        _pool.Seekers.First(s => s.UserId == "user1")?.WavesMissed.Should().Be(1);
    }

    [Fact]
    public void CalculateMatches_no_seekers_returns_empty()
    {
        var matches = _pool.CalculateMatches();
        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_single_seeker_no_match()
    {
        _pool.AddSeek("user1", 1500);
        var matches = _pool.CalculateMatches();
        matches.Should().BeEmpty();
        _pool.Seekers.First(s => s.UserId == "user1").WavesMissed.Should().Be(1);
    }

    [Fact]
    public void CalculateMatches_match_prefers_closer_rating_when_waves_missed_equal()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek("user2", 1210);
        _pool.AddSeek("user3", 1215);

        // Both user2 and user3 can match with user1, but user2 has closer rating difference
        var matches = _pool.CalculateMatches();
        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_handles_multiple_pairs()
    {
        _pool.AddSeek("user1", 1200);
        _pool.AddSeek("user2", 1205);
        _pool.AddSeek("user3", 1210);
        _pool.AddSeek("user4", 1195);

        var matches = _pool.CalculateMatches();

        // Should create two matches
        matches.Count.Should().Be(2);

        var allMatchedUsers = matches.SelectMany(m => new[] { m.userId1, m.userId2 }).ToList();
        allMatchedUsers.Should().Contain(["user1", "user2", "user3", "user4"]);
    }
}
