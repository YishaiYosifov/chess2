using Chess2.Api.Matchmaking.Models;
using ErrorOr;
using StackExchange.Redis;

namespace Chess2.Api.Matchmaking.Repositories;

public static class MatchmakingUserHashFields
{
    public const string StartedSeekingTimestamp = "started_seeking_timestamp";
    public const string TimeControl = "time_control";
    public const string Increment = "increment";
}

public interface IMatchmakingRepository
{
    Task<ErrorOr<Success>> CancelSeekAsync(SeekInfo seek);
    Task CreateSeekAsync(
        string userId,
        int rating,
        int timeControl,
        int increment,
        long seekStartedAt
    );
    Task<SeekInfo?> GetUserSeekingInfo(string userId);
    Task<string?> SearchExistingSeekAsync(
        int minRating,
        int maxRating,
        int timeControl,
        int increment
    );
}

public class MatchmakingRepository(IConnectionMultiplexer redisConn) : IMatchmakingRepository
{
    private readonly IDatabase _redis = redisConn.GetDatabase();

    private const string MatchmakingSetName = "matchmaking";
    private const string MatchmakingUsersHashName = "matchmaking_users";

    private static string GetUserSetName(string id) => $"{MatchmakingUsersHashName}:{id}";

    private static string GetQueueName(int timeControl, int increment) =>
        $"{MatchmakingSetName}:{timeControl}+{increment}";

    public async Task CreateSeekAsync(
        string userId,
        int rating,
        int timeControl,
        int increment,
        long seekStartedAtTimestamp
    )
    {
        var queueName = GetQueueName(timeControl, increment);
        await _redis.SortedSetAddAsync(queueName, userId, rating);

        var userSetName = GetUserSetName(userId);
        var userMatchmakingData = new HashEntry[]
        {
            new(MatchmakingUserHashFields.StartedSeekingTimestamp, seekStartedAtTimestamp),
            new(MatchmakingUserHashFields.TimeControl, timeControl),
            new(MatchmakingUserHashFields.Increment, increment),
        };
        await _redis.HashSetAsync(userSetName, userMatchmakingData);
    }

    public async Task<ErrorOr<Success>> CancelSeekAsync(SeekInfo seek)
    {
        var queueName = GetQueueName(seek.TimeControl, seek.Increment);
        var userSetName = GetUserSetName(seek.UserId);

        await _redis.SortedSetRemoveAsync(queueName, seek.UserId);
        await _redis.KeyDeleteAsync(userSetName);
        return Result.Success;
    }

    public async Task<SeekInfo?> GetUserSeekingInfo(string userId)
    {
        var userSetName = GetUserSetName(userId);
        var isUserSeeking = await _redis.KeyExistsAsync(userSetName);
        if (!isUserSeeking)
            return null;

        var timeControl = (int)
            await _redis.HashGetAsync(userSetName, MatchmakingUserHashFields.TimeControl);
        var increment = (int)
            await _redis.HashGetAsync(userSetName, MatchmakingUserHashFields.Increment);
        var startedAt = (long)
            await _redis.HashGetAsync(
                userSetName,
                MatchmakingUserHashFields.StartedSeekingTimestamp
            );

        var seekInfo = new SeekInfo(userId, timeControl, increment, startedAt);
        return seekInfo;
    }

    public async Task<string?> SearchExistingSeekAsync(
        int minRating,
        int maxRating,
        int timeControl,
        int increment
    )
    {
        var queueName = GetQueueName(timeControl, increment);
        var matchedUserId = await _redis.SortedSetRangeByScoreAsync(
            queueName,
            minRating,
            maxRating
        );
        return matchedUserId.FirstOrDefault();
    }
}
