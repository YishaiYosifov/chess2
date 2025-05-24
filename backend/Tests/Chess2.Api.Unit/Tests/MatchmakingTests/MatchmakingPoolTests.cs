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
        _pool.AddSeek("user2", 1200 + _settings.StartingMatchRatingDifference - 1);

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
                - 1
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
        _pool.AddSeek("user3", 1205);

        // user1 should match with user2 (oldest), user3 left unmatched
        var matches = _pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().BeEquivalentTo(("user1", "user2"));
        _pool.Seekers.First(s => s.UserId == "user3")?.WavesMissed.Should().Be(1);
    }
}
