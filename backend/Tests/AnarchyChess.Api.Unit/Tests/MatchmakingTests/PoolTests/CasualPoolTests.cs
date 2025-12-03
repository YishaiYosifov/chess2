using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services.Pools;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class CasualPoolTests : BasePoolTests<CasualMatchmakingPool>
{
    protected override CasualMatchmakingPool Pool { get; } = new();

    protected override CasualSeeker AddSeeker(UserId userId)
    {
        CasualSeeker seeker = new(
            UserId: userId,
            UserName: userId,
            ExcludeUserIds: [],
            CreatedAt: DateTime.UtcNow
        );
        Pool.AddSeeker(seeker);
        return seeker;
    }

    [Fact]
    public void CalculateMatches_returns_empty_list_when_no_seekers()
    {
        var matches = Pool.CalculateMatches();
        matches.Should().BeEmpty();
        Pool.Seekers.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_returns_all_pairs_with_even_number_of_seekers()
    {
        var seeker1 = AddSeeker("User1");
        var seeker2 = AddSeeker("User2");
        var seeker3 = AddSeeker("User3");
        var seeker4 = AddSeeker("User4");

        var matches = Pool.CalculateMatches();

        matches.Should().HaveCount(2);
        matches.Should().BeEquivalentTo([(seeker1, seeker2), (seeker3, seeker4)]);
    }

    [Fact]
    public void CalculateMatches_leaves_last_seeker_unmatched_with_odd_number_of_seekers()
    {
        var seeker1 = AddSeeker("User1");
        var seeker2 = AddSeeker("User2");
        AddSeeker("User3");

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().Be((seeker1, seeker2));
    }

    [Fact]
    public void CalculateMatches_does_not_match_if_seekers_are_blocked()
    {
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker()
            .RuleFor(x => x.ExcludeUserIds, [seeker1.UserId])
            .Generate();
        Pool.AddSeeker(seeker1);
        Pool.AddSeeker(seeker2);

        var matches = Pool.CalculateMatches();

        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_matches_only_compatible_seekers()
    {
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker()
            .RuleFor(x => x.ExcludeUserIds, [seeker1.UserId])
            .Generate();
        var seeker3 = new CasualSeekerFaker().Generate();

        Pool.AddSeeker(seeker1);
        Pool.AddSeeker(seeker2);
        Pool.AddSeeker(seeker3);

        var matches = Pool.CalculateMatches();

        // user1 and user3 can match, user2 blocks user1 so no match
        matches.Should().ContainSingle().Which.Should().Be((seeker1, seeker3));
    }
}
