using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class RatedPoolTests : BasePoolTests<RatedMatchmakingPool>
{
    private const int AllowedMatchRatingDifference = 300;
    protected override RatedMatchmakingPool Pool { get; } = new();

    private void AddSeeker(string userId, int rating)
    {
        RatedSeeker seeker = new(
            userId,
            userId,
            BlockedUserIds: [],
            Rating: new SeekerRating(rating, AllowedMatchRatingDifference)
        );
        Pool.TryAddSeek(seeker);
    }

    protected override void AddSeek(string userId) => AddSeeker(userId, 1200);

    [Fact]
    public void CalculateMatches_matches_compatible_seekers_within_range()
    {
        AddSeeker("user1", 1200);
        AddSeeker("user2", 1200 + AllowedMatchRatingDifference - 1);

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle();
        var (a, b) = matches.Single();
        (a.UserId, b.UserId).Should().BeEquivalentTo(("user1", "user2"));
    }

    [Fact]
    public void CalculateMatches_does_not_match_outside_rating_range()
    {
        AddSeeker("user1", 1200);
        AddSeeker("user2", 1200 + AllowedMatchRatingDifference + 1);

        Pool.CalculateMatches().Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_respects_block_lists()
    {
        RatedSeeker blocked = new(
            "user1",
            "user1",
            ["user2"],
            new SeekerRating(1200, AllowedMatchRatingDifference)
        );
        RatedSeeker normal = new(
            "user2",
            "user2",
            [],
            new SeekerRating(1200, AllowedMatchRatingDifference)
        );
        Pool.TryAddSeek(blocked);
        Pool.TryAddSeek(normal);

        Pool.CalculateMatches().Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_prioritizes_older_seeks_due_to_missed_waves()
    {
        // user1 and user2 will match first (closer rating)
        AddSeeker("user1", 1200);
        AddSeeker("user2", 1210);
        AddSeeker("user3", 1220);

        var matches1 = Pool.CalculateMatches();
        matches1
            .Should()
            .ContainSingle()
            .And.OnlyContain(m => m.seeker1.UserId != "user3" && m.seeker2.UserId != "user3");

        AddSeeker("user1", 1200);
        AddSeeker("user2", 1210);

        var matches2 = Pool.CalculateMatches();

        // user3 should now get matched due to lower score
        matches2.Should().ContainSingle();
        matches2
            .SelectMany(m => new[] { m.seeker1.UserId, m.seeker2.UserId })
            .Should()
            .Contain("user3");
    }

    [Fact]
    public void CalculateMatches_matches_multiple_pairs()
    {
        AddSeeker("user1", 1200);
        AddSeeker("user2", 1205);
        AddSeeker("user3", 1250);
        AddSeeker("user4", 1245);

        var matches = Pool.CalculateMatches();

        matches.Should().HaveCount(2);
        var usersMatched = matches
            .SelectMany(m => new[] { m.seeker1.UserId, m.seeker2.UserId })
            .ToList();
        usersMatched.Should().BeEquivalentTo("user1", "user2", "user3", "user4");
    }

    [Fact]
    public void CalculateMatches_removes_matched_users()
    {
        AddSeeker("user1", 1200);
        AddSeeker("user2", 1200);
        AddSeeker("user3", 1300);

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
        AddSeeker("user1", 1400);
        Pool.CalculateMatches().Should().BeEmpty();
    }
}
