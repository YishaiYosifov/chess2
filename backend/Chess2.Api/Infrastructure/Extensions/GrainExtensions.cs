using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;

namespace Chess2.Api.Infrastructure.Extensions;

public static class GrainExtensions
{
    public static IMatchmakingGrain GetMatchmakingGrain(this IGrainFactory factory, PoolKey pool)
    {
        return pool.PoolType switch
        {
            PoolType.Rated => factory.GetGrain<IMatchmakingGrain<IRatedMatchmakingPool>>(
                pool.ToGrainKey()
            ),
            PoolType.Casual => factory.GetGrain<IMatchmakingGrain<ICasualMatchmakingPool>>(
                pool.ToGrainKey()
            ),
            _ => throw new InvalidOperationException($"Unsupported pool type: {pool.PoolType}"),
        };
    }

    public static TGrainInterface AsSafeReference<TGrainInterface>(this IAddressable grain)
    {
        try
        {
            return grain.AsReference<TGrainInterface>();
        }
        catch (ArgumentException ex)
            when (ex.Message.Contains("Passing a half baked grain as an argument"))
        {
            return (TGrainInterface)grain;
        }
    }
}
