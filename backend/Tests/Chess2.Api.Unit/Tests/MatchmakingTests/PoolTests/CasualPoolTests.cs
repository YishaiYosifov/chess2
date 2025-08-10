using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.PoolTests;

public class CasualPoolTests : BasePoolTests<CasualMatchmakingPool>
{
    protected override CasualMatchmakingPool Pool { get; } = new();

    protected override void AddSeek(string userId)
    {
        Seeker seek = new(userId, userId, BlockedUserIds: []);
        Pool.TryAddSeek(seek);
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
        AddSeek("User1");
        AddSeek("User2");
        AddSeek("User3");
        AddSeek("User4");

        var matches = Pool.CalculateMatches();

        matches.Should().HaveCount(2);
        var matchedUserIds = matches
            .SelectMany(m => new[] { m.seeker1.UserId, m.seeker2.UserId })
            .ToList();
        matchedUserIds.Should().Contain(["User1", "User2", "User3", "User4"]);

        Pool.Seekers.Should().BeEmpty();
    }

    [Fact]
    public void CalculateMatches_leaves_last_seeker_unmatched_with_odd_number_of_seekers()
    {
        AddSeek("User1");
        AddSeek("User2");
        AddSeek("User3");

        var matches = Pool.CalculateMatches();

        matches.Should().ContainSingle();
        var matchedIds = matches
            .SelectMany(m => new[] { m.seeker1.UserId, m.seeker2.UserId })
            .ToList();
        matchedIds.Should().Contain(["User1", "User2"]);

        Pool.Seekers.Should().ContainSingle().Which.Should().Be("User3");
    }

    [Fact]
    public void CalculateMatches_does_not_match_if_seekers_are_blocked()
    {
        Seeker seeker1 = new("user1", "User1", ["user2"]);
        Seeker seeker2 = new("user2", "User2", []);
        Pool.TryAddSeek(seeker1);
        Pool.TryAddSeek(seeker2);

        var matches = Pool.CalculateMatches();

        matches.Should().BeEmpty();
        Pool.Seekers.Should().Contain([seeker1, seeker2]);
    }

    [Fact]
    public void CalculateMatches_matches_only_compatible_seekers()
    {
        Seeker user1 = new("user1", "User1", []);
        Seeker user2 = new("user2", "User2", []);
        Seeker user3 = new("user3", "User3", ["user1"]);

        Pool.TryAddSeek(user1);
        Pool.TryAddSeek(user2);
        Pool.TryAddSeek(user3);

        var matches = Pool.CalculateMatches();

        // user1 and user2 can match, user3 blocks user1 so no match
        matches.Should().ContainSingle();
        var (s1, s2) = matches.Single();
        (
            s1.UserId == "user1" && s2.UserId == "user2"
            || s1.UserId == "user2" && s2.UserId == "user1"
        )
            .Should()
            .BeTrue();

        Pool.Seekers.Should().ContainSingle().Which.Should().Be("user3");
    }
}
