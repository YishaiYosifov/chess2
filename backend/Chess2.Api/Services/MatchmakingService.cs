using Chess2.Api.Models.Entities;
using StackExchange.Redis;

namespace Chess2.Api.Services;

public interface IMatchmakingService
{
    public Task SeekAsync(AuthedUser user, int timeControl, int increment);
    public Task SeekGuestAsync(string id, int timeControl, int increment);

    public Task CancelSeekAsync(string id);
}

public class MatchmakingService(IConnectionMultiplexer redisConn) : IMatchmakingService
{
    private readonly IDatabase _redis = redisConn.GetDatabase();

    public Task SeekAsync(AuthedUser user, int timeControl, int increment)
    {
        throw new NotImplementedException();
    }

    public Task SeekGuestAsync(string id, int timeControl, int increment)
    {
        throw new NotImplementedException();
    }

    public Task CancelSeekAsync(string id)
    {
        throw new NotImplementedException();
    }
}
