using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Infrastructure.Extensions;

public static class GrainFactoryExtensions
{
    public static IMatchmakingGrain GetMatchmakingGrain(this IGrainFactory factory, PoolKey pool)
    {
        return pool.PoolType switch
        {
            PoolType.Rated => factory.GetGrain<IRatedMatchmakingGrain>(pool.ToGrainKey()),
            PoolType.Casual => factory.GetGrain<ICasualMatchmakingGrain>(pool.ToGrainKey()),
            _ => throw new InvalidOperationException($"Unsupported pool type: {pool.PoolType}"),
        };
    }
}
