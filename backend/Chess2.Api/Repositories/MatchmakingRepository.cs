using ErrorOr;
using StackExchange.Redis;

namespace Chess2.Api.Repositories;

public interface IMatchmakingRepository
{
    Task<ErrorOr<Success>> CancelSeekAsync(string userId);
    Task CreateSeekAsync(string userId, int rating, int timeControl, int increment);
    Task<string?> SearchExistingSeekAsync(
        int rating,
        int ratingRange,
        int timeControl,
        int increment
    );
}

public static class MatchmakingUserHashFields
{
    public const string StartedSeeking = "started_seeking";
    public const string TimeControl = "time_control";
    public const string Increment = "increment";
}

public class MatchmakingRepository(
    ILogger<MatchmakingRepository> logger,
    IConnectionMultiplexer redisConn
) : IMatchmakingRepository
{
    private readonly IDatabase _redis = redisConn.GetDatabase();
    private readonly ILogger<MatchmakingRepository> _logger = logger;

    private const string MatchmakingSetName = "matchmaking";
    private const string MatchmakingUsersHashName = "matchmaking_users";

    private static string GetUserSetName(string id) => $"{MatchmakingUsersHashName}:{id}";

    private static string GetQueueName(int timeControl, int increment) =>
        $"{MatchmakingSetName}:{timeControl}+{increment}";

    public async Task CreateSeekAsync(string userId, int rating, int timeControl, int increment)
    {
        var queueName = GetQueueName(timeControl, increment);
        await _redis.SortedSetAddAsync(queueName, userId, rating);

        var userSetName = GetUserSetName(userId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var userMatchmakingData = new HashEntry[]
        {
            new(MatchmakingUserHashFields.StartedSeeking, now),
            new(MatchmakingUserHashFields.TimeControl, timeControl),
            new(MatchmakingUserHashFields.Increment, increment),
        };
        await _redis.HashSetAsync(userSetName, userMatchmakingData);
    }

    public async Task<ErrorOr<Success>> CancelSeekAsync(string userId)
    {
        var userSetName = GetUserSetName(userId);
        var isUserSeeking = await _redis.KeyExistsAsync(userSetName);
        if (!isUserSeeking)
        {
            _logger.LogError("Could not find user {UserId} matchmaking data", userId);
            return Error.NotFound();
        }

        var timeControl = (int)
            await _redis.HashGetAsync(userId, MatchmakingUserHashFields.TimeControl);
        var increment = (int)await _redis.HashGetAsync(userId, MatchmakingUserHashFields.Increment);
        var queueName = GetQueueName(timeControl, increment);

        await _redis.SortedSetRemoveAsync(queueName, userId);
        await _redis.KeyDeleteAsync(userSetName);
        return Result.Success;
    }

    public async Task<string?> SearchExistingSeekAsync(
        int rating,
        int ratingRange,
        int timeControl,
        int increment
    )
    {
        var minRating = rating - ratingRange;
        var maxRating = rating + ratingRange;
        var queueName = GetQueueName(timeControl, increment);
        var matchedUserId = await _redis.SortedSetRangeByScoreAsync(
            queueName,
            minRating,
            maxRating
        );
        return matchedUserId.FirstOrDefault();
    }
}
