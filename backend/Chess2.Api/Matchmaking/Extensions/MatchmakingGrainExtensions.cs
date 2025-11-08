using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;

namespace Chess2.Api.Matchmaking.Extensions;

public static class MatchmakingGrainExtensions
{
    public static IMatchmakingGrain GetMatchmakingGrain(this IGrainFactory factory, PoolKey pool)
    {
        return pool.PoolType switch
        {
            PoolType.Rated => factory.GetGrain<IMatchmakingGrain<RatedMatchmakingPool>>(
                pool.ToGrainKey()
            ),
            PoolType.Casual => factory.GetGrain<IMatchmakingGrain<CasualMatchmakingPool>>(
                pool.ToGrainKey()
            ),
            _ => throw new InvalidOperationException($"Unsupported pool type: {pool.PoolType}"),
        };
    }
}
