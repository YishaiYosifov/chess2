using AnarchyChess.Api.Matchmaking.Grains;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services.Pools;

namespace AnarchyChess.Api.Matchmaking.Extensions;

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
