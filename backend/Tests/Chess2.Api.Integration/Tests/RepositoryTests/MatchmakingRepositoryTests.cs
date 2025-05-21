using Chess2.Api.Matchmaking.Repositories;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Chess2.Api.Integration.Tests.RepositoryTests;

public class MatchmakingRepositoryTests : BaseIntegrationTest
{
    private readonly IDatabase _redisDb;
    private readonly IMatchmakingRepository _repository;

    public MatchmakingRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        var redisConn = Scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        _redisDb = redisConn.GetDatabase();

        _repository = Scope.ServiceProvider.GetRequiredService<IMatchmakingRepository>();
    }

    [Fact]
    public async Task CreateSeekAsync_stores_seek_data_in_redis()
    {
        var userId = Guid.NewGuid().ToString();
        int rating = 1500,
            timeControl = 5,
            increment = 3;
        var startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _repository.CreateSeekAsync(userId, rating, timeControl, increment, startedAt);

        var queueName = $"matchmaking:{timeControl}+{increment}";
        var userSetName = $"matchmaking_users:{userId}";

        // Check sorted set
        var score = await _redisDb.SortedSetScoreAsync(queueName, userId);
        score.Should().Be(rating);

        // Check hash
        var hash = await _redisDb.HashGetAllAsync(userSetName);
        hash.Should().HaveCount(3);
        hash.Should()
            .Contain(h =>
                h.Name == MatchmakingUserHashFields.StartedSeekingTimestamp && h.Value == startedAt
            );
        hash.Should()
            .Contain(h =>
                h.Name == MatchmakingUserHashFields.TimeControl && h.Value == timeControl
            );
        hash.Should()
            .Contain(h => h.Name == MatchmakingUserHashFields.Increment && h.Value == increment);
    }

    [Fact]
    public async Task GetUserSeekingInfo_returns_seek_info_when_it_exists()
    {
        var userId = Guid.NewGuid().ToString();
        int rating = 1600,
            timeControl = 10,
            increment = 5;
        long startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _repository.CreateSeekAsync(userId, rating, timeControl, increment, startedAt);

        var seekInfo = await _repository.GetUserSeekingInfo(userId);

        seekInfo.Should().NotBeNull();
        seekInfo.UserId.Should().Be(userId);
        seekInfo.TimeControl.Should().Be(timeControl);
        seekInfo.Increment.Should().Be(increment);
        seekInfo.StartedAtTimestamp.Should().Be(startedAt);
    }

    [Fact]
    public async Task GetUserSeekingInfo_returns_null_when_seek_info_doesnt_exist()
    {
        var seekInfo = await _repository.GetUserSeekingInfo("nonexistent-user");
        seekInfo.Should().BeNull();
    }

    [Fact]
    public async Task CancelSeekAsync_deletes_seek_data()
    {
        var userId = Guid.NewGuid().ToString();
        int rating = 1700,
            timeControl = 3,
            increment = 2;
        long startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _repository.CreateSeekAsync(userId, rating, timeControl, increment, startedAt);
        var seekInfo = await _repository.GetUserSeekingInfo(userId);
        seekInfo.Should().NotBeNull();

        var result = await _repository.CancelSeekAsync(seekInfo!);
        result.IsError.Should().BeFalse();

        var queueName = $"matchmaking:{timeControl}+{increment}";
        var userSetName = $"matchmaking_users:{userId}";
        var score = await _redisDb.SortedSetScoreAsync(queueName, userId);
        var exists = await _redisDb.KeyExistsAsync(userSetName);

        score.Should().BeNull();
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SearchExistingSeekAsync_finds_user_in_rating_range()
    {
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        int rating1 = 1200,
            rating2 = 1800,
            timeControl = 15,
            increment = 10;
        long startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _repository.CreateSeekAsync(userId1, rating1, timeControl, increment, startedAt);
        await _repository.CreateSeekAsync(userId2, rating2, timeControl, increment, startedAt);

        var foundUser = await _repository.SearchExistingSeekAsync(
            1100,
            1300,
            timeControl,
            increment
        );
        foundUser.Should().Be(userId1);

        var foundUser2 = await _repository.SearchExistingSeekAsync(
            1700,
            1900,
            timeControl,
            increment
        );
        foundUser2.Should().Be(userId2);

        var notFound = await _repository.SearchExistingSeekAsync(
            2000,
            2100,
            timeControl,
            increment
        );
        notFound.Should().BeNull();
    }
}
