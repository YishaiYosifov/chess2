using StackExchange.Redis;

namespace Chess2.Api.Services;

public interface IMatchmakingService
{
    public Task Seek(string id, int timeControl, int increment, int elo);
    public Task CancelSeek(string id);
}

public class MatchmakingService(IConnectionMultiplexer redisConn) : IMatchmakingService
{
    private readonly IDatabase _redis = redisConn.GetDatabase();

    public Task Seek(string id, int timeControl, int increment, int elo)
    {
        throw new NotImplementedException();
    }

    public Task CancelSeek(string id)
    {
        throw new NotImplementedException();
    }
}
