using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class RatedPoolTests : BasePoolTests<RatedMatchmakingPool>
{
    private const int AllowedMatchRatingDifference = 300;
    protected override RatedMatchmakingPool Pool { get; } = new();

    private void AddSeek(string userId, int rating)
    {
        var seek = new RatedSeek(
            userId,
            userId,
            BlockedUserIds: [],
            Rating: new SeekRating(rating, AllowedMatchRatingDifference)
        );
        Pool.TryAddSeek(seek);
    }

    protected override void AddSeek(string userId) => AddSeek(userId, 1200);

    [Fact]
    public void CalculateMatches_matches_compatible_seekers_within_range()
    {
        AddSeek("user1", 1200);
        AddSeek("user2", 1200 + AllowedMatchRatingDifference - 1);

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle();
        var (a, b) = matches.Single();
        (a.UserId, b.UserId).Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_does_not_match_outside_rating_range()
    {
        AddSeek("user1", 1200);
        AddSeek("user2", 1200 + AllowedMatchRatingDifference + 1);

        Pool.CalculateMatches().Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_respects_block_lists()
    {
        RatedSeek blocked = new(
            "user1",
            "user1",
            ["user2"],
            new SeekRating(1200, AllowedMatchRatingDifference)
        );
        RatedSeek normal = new(
            "user2",
            "user2",
            [],
            new SeekRating(1200, AllowedMatchRatingDifference)
        );
        Pool.TryAddSeek(blocked);
        Pool.TryAddSeek(normal);

        Pool.CalculateMatches().Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_prioritizes_older_seeks_due_to_missed_waves()
    {
        // user1 and user2 will match first (closer rating)
        AddSeek("user1", 1200);
        AddSeek("user2", 1210);
        AddSeek("user3", 1220);

        var matches1 = Pool.CalculateMatches();
        matches1
            .Should()
            .ContainSingle()
            .And.OnlyContain(m => m.seek1.UserId != "user3" && m.seek2.UserId != "user3");

        AddSeek("user1", 1200);
        AddSeek("user2", 1210);

        var matches2 = Pool.CalculateMatches();

        // user3 should now get matched due to lower score
        matches2.Should().ContainSingle();
        matches2
            .SelectMany(m => new[] { m.seek1.UserId, m.seek2.UserId })
            .Should()
            .Contain("user3");
    }

    [Fact]
    public void CalculateMatches_matches_multiple_pairs()
    {
        AddSeek("user1", 1200);
        AddSeek("user2", 1205);
        AddSeek("user3", 1250);
        AddSeek("user4", 1245);

        var matches = Pool.CalculateMatches();

        matches.Should().HaveCount(2);
        var usersMatched = matches
            .SelectMany(m => new[] { m.seek1.UserId, m.seek2.UserId })
            .ToList();
        usersMatched.Should().BeEquivalentTo("user1", "user2", "user3", "user4");
    }

    [Fact]
    public void CalculateMatches_removes_matched_users()
    {
        AddSeek("user1", 1200);
        AddSeek("user2", 1200);
        AddSeek("user3", 1300);

        Pool.CalculateMatches();

        Pool.Seekers.Should().ContainSingle().Which.Should().Be("user3");
    }

    [Fact]
    public void CalculateMatches_returns_empty_when_no_seekers()
    {
        Pool.CalculateMatches().Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_returns_empty_with_only_one_seeker()
    {
        AddSeek("user1", 1400);
        Pool.CalculateMatches().Should().BeEmpty();
    }
}
