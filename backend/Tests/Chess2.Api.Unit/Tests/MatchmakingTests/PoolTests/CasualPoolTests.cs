using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class CasualPoolTests : BasePoolTests<CasualMatchmakingPool>
{
    protected override CasualMatchmakingPool Pool { get; } = new();

    protected override Seeker AddSeek(string userId)
    {
        Seeker seeker = new(userId, userId, BlockedUserIds: []);
        Pool.TryAddSeek(seeker);
        return seeker;
    }

    [Fact]
    public void CalculateMatches_returns_empty_list_when_no_seekers()
    {
        var matches = Pool.CalculateMatches();
        matches.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_returns_all_pairs_with_even_number_of_seekers()
    {
        var seeker1 = AddSeek("User1");
        var seeker2 = AddSeek("User2");
        var seeker3 = AddSeek("User3");
        var seeker4 = AddSeek("User4");

        var matches = Pool.CalculateMatches();

        matches.Should().HaveCount(2);
        matches.Should().BeEquivalentTo([(seeker1, seeker2), (seeker3, seeker4)]);

        Pool.Seekers.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_leaves_last_seeker_unmatched_with_odd_number_of_seekers()
    {
        var seeker1 = AddSeek("User1");
        var seeker2 = AddSeek("User2");
        var seeker3 = AddSeek("User3");

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle().Which.Should().Be((seeker1, seeker2));

        Pool.Seekers.Should().ContainSingle().Which.Should().Be(seeker3);
    }

    [Fact]
    public void CalculateMatches_does_not_match_if_seekers_are_blocked()
    {
        Seeker seeker1 = new(UserId: "user1", UserName: "User1", BlockedUserIds: ["user2"]);
        Seeker seeker2 = new(UserId: "user2", UserName: "User2", BlockedUserIds: []);
        Pool.TryAddSeek(seeker1);
        Pool.TryAddSeek(seeker2);

        var matches = Pool.CalculateMatches();

        matches.Should().BeEmpty();
        Pool.Seekers.Should().Contain([seeker1, seeker2]);
    }

    [Fact]
    public void CalculateMatches_matches_only_compatible_seekers()
    {
        Seeker seeker1 = new(UserId: "user1", UserName: "User1", BlockedUserIds: []);
        Seeker seeker2 = new(UserId: "user2", UserName: "User2", BlockedUserIds: []);
        Seeker seeker3 = new(UserId: "user3", UserName: "User3", BlockedUserIds: ["user1"]);

        Pool.TryAddSeek(seeker1);
        Pool.TryAddSeek(seeker2);
        Pool.TryAddSeek(seeker3);

        var matches = Pool.CalculateMatches();

        // user1 and user2 can match, user3 blocks user1 so no match
        matches.Should().ContainSingle().Which.Should().Be((seeker1, seeker2));

        Pool.Seekers.Should().ContainSingle().Which.Should().Be(seeker3);
    }
}
