using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class RatedPoolTests : BasePoolTests<RatedMatchmakingPool>
{
    private const int AllowedMatchRatingDifference = 300;
    protected override RatedMatchmakingPool Pool { get; } = new();

    private RatedSeeker AddSeeker(
        UserId userId,
        int rating,
        int? allowedDifference = AllowedMatchRatingDifference
    )
    {
        RatedSeeker seeker = new(
            userId,
            userId,
            ExcludeUserIds: [],
            Rating: new SeekerRating(rating, allowedDifference, TimeControl: TimeControl.Blitz),
            CreatedAt: DateTime.UtcNow
        );
        Pool.AddSeeker(seeker);
        return seeker;
    }

    protected override Seeker AddSeeker(UserId userId) => AddSeeker(userId, 1200);

    [Fact]
    public void CalculateMatches_matches_seekers_when_allowed_difference_is_null()
    {
        var seeker1 = AddSeeker("user1", 1200, null);
        var seeker2 = AddSeeker("user2", 1800, null);

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().Be((seeker1.UserId, seeker2.UserId));
    }

    [Fact]
    public void CalculateMatches_matches_compatible_seekers_within_range()
    {
        var seeker1 = AddSeeker("user1", 1200);
        var seeker2 = AddSeeker("user2", 1200 + AllowedMatchRatingDifference - 1);

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().Be((seeker1.UserId, seeker2.UserId));
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
        RatedSeeker blocked = new RatedSeekerFaker(rating: 1200).Generate();
        RatedSeeker normal = new RatedSeekerFaker(rating: 1200)
            .RuleFor(x => x.ExcludeUserIds, [blocked.UserId])
            .Generate();
        Pool.AddSeeker(blocked);
        Pool.AddSeeker(normal);

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
            .And.OnlyContain(m => m.User1 != "user3" && m.User2 != "user3");

        AddSeeker("user1", 1200);
        AddSeeker("user2", 1210);

        var matches2 = Pool.CalculateMatches();

        // user3 should now get matched due to lower score
        matches2.Should().ContainSingle();
        matches2.SelectMany(m => new[] { m.User1, m.User2 }).Should().Contain("user3");
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
        var usersMatched = matches.SelectMany(m => new[] { m.User1, m.User2 }).ToList();
        usersMatched.Should().BeEquivalentTo<UserId>(["user1", "user2", "user3", "user4"]);
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
