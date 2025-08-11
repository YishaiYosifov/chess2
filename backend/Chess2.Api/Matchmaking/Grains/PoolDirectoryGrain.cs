using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IPoolDirectoryGrain")]
public interface IPoolDirectoryGrain : IGrainWithIntegerKey
{
    [Alias("GetSeekersForAsync")]
    Task<Dictionary<PoolKey, List<Seeker>>> GetSeekersForAsync(Seeker seeker);

    [Alias("RegisterPoolAsync")]
    Task RegisterPoolAsync(PoolKey poolKey);

    [Alias("UnregisterPoolAsync")]
    Task UnregisterPoolAsync(PoolKey poolKey);
}

[KeepAlive]
public class PoolDirectoryGrain : Grain, IPoolDirectoryGrain
{
    private readonly HashSet<PoolKey> _pools = [];

    public async Task<Dictionary<PoolKey, List<Seeker>>> GetSeekersForAsync(Seeker seeker)
    {
        var seekResults = await Task.WhenAll(
            _pools.Select(async pool => new
            {
                Pool = pool,
                Seeks = (
                    await GrainFactory
                        .GetGrain<IMatchmakingGrain>(pool.ToGrainKey())
                        .GetMatchingSeekersForAsync(seeker)
                ).ToList(),
            })
        );

        var seeksByPool = seekResults.ToDictionary(x => x.Pool, x => x.Seeks);
        return seeksByPool;
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
