using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IPoolDirectoryGrain")]
public interface IPoolDirectoryGrain : IGrainWithIntegerKey
{
    [Alias("GetSeekersForAsync")]
    Task<Dictionary<PoolKey, List<Seeker>>> GetAllSeekersAsync();

    [Alias("RegisterPoolAsync")]
    Task RegisterPoolAsync(PoolKey poolKey);

    [Alias("UnregisterPoolAsync")]
    Task UnregisterPoolAsync(PoolKey poolKey);
}

[KeepAlive]
public class PoolDirectoryGrain : Grain, IPoolDirectoryGrain
{
    private readonly HashSet<PoolKey> _pools = [];

    public async Task<Dictionary<PoolKey, List<Seeker>>> GetAllSeekersAsync()
    {
        var seekResults = await Task.WhenAll(
            _pools.Select(async pool => new
            {
                Pool = pool,
                Seeks = (await ResolvePoolGrain(pool).GetSeekersAsync()).ToList(),
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

    private IMatchmakingGrain ResolvePoolGrain(PoolKey pool)
    {
        return pool.PoolType switch
        {
            PoolType.Rated => GrainFactory.GetGrain<IRatedMatchmakingGrain>(pool.ToGrainKey()),
            PoolType.Casual => GrainFactory.GetGrain<ICasualMatchmakingGrain>(pool.ToGrainKey()),
            _ => throw new InvalidOperationException($"Unsupported pool type: {pool.PoolType}"),
        };
    }
}
