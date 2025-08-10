using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IPoolDirectoryGrain")]
public interface IPoolDirectoryGrain : IGrainWithIntegerKey
{
    [Alias("GetSeekersForAsync")]
    Task<List<Seeker>> GetSeekersForAsync(Seeker seeker);

    [Alias("RegisterPoolAsync")]
    Task RegisterPoolAsync(PoolKey poolKey);

    [Alias("UnregisterPoolAsync")]
    Task UnregisterPoolAsync(PoolKey poolKey);
}

public class PoolDirectoryGrain : Grain, IPoolDirectoryGrain
{
    private readonly HashSet<PoolKey> _pools = [];

    public async Task<List<Seeker>> GetSeekersForAsync(Seeker seeker)
    {
        var matches = await Task.WhenAll(
            _pools.Select(pool =>
                GrainFactory
                    .GetGrain<IMatchmakingGrain>(pool.ToGrainKey())
                    .GetMatchingSeekersForAsync(seeker)
            )
        );
        return [.. matches.SelectMany(x => x)];
    }

    public Task RegisterPoolAsync(PoolKey poolKey)
    {
        _pools.Add(poolKey);
        return Task.CompletedTask;
    }

    public Task UnregisterPoolAsync(PoolKey poolKey)
    {
        _pools.Remove(poolKey);
        return Task.CompletedTask;
    }
}
