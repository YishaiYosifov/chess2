using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekRatedAsync(AuthedUser user, string connectionId, TimeControlSettings timeControl);

    Task SeekCasualAsync(string userId, string connectionId, TimeControlSettings timeControl);

    Task CancelSeekAsync(string userId, string connectionId);
}

public class MatchmakingService(
    IGrainFactory grains,
    IRatingService ratingService,
    ITimeControlTranslator secondsToTimeControl
) : IMatchmakingService
{
    private readonly IGrainFactory _grains = grains;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;

    public async Task SeekRatedAsync(
        AuthedUser user,
        string connectionId,
        TimeControlSettings timeControl
    )
    {
        var rating = await _ratingService.GetRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(timeControl.BaseSeconds)
        );

        PoolKey poolKey = new(PoolType.Rated, timeControl);

        var matchmakingGrain = _grains.GetGrain<IRatedMatchmakingGrain>(poolKey.ToGrainKey());
        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(user.Id);

        await matchmakingGrain.CreateSeekAsync(user.Id, rating, playerSessionGrain);
        await playerSessionGrain.RegisterSeekAsync(connectionId, poolKey);
    }

    public async Task SeekCasualAsync(
        string userId,
        string connectionId,
        TimeControlSettings timeControl
    )
    {
        PoolKey poolKey = new(PoolType.Casual, timeControl);
        var matchmakingGrain = _grains.GetGrain<ICasualMatchmakingGrain>(poolKey.ToGrainKey());
        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(userId);

        await matchmakingGrain.CreateSeekAsync(userId, playerSessionGrain);
        await playerSessionGrain.RegisterSeekAsync(connectionId, poolKey);
    }

    public async Task CancelSeekAsync(string userId, string connectionId)
    {
        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        var poolKey = await playerSessionGrain.RemoveSeekForConnectionAsync(connectionId);
        if (poolKey is null)
            return;

        var poolGrain = ResolvePoolGrainForSeek(poolKey);
        await poolGrain.CancelSeekAsync(userId);
    }

    private IMatchmakingGrain ResolvePoolGrainForSeek(PoolKey poolKey)
    {
        return poolKey.PoolType switch
        {
            PoolType.Rated => _grains.GetGrain<IRatedMatchmakingGrain>(poolKey.ToGrainKey()),
            PoolType.Casual => _grains.GetGrain<ICasualMatchmakingGrain>(poolKey.ToGrainKey()),
            _ => throw new InvalidOperationException($"Unsupported pool type: {poolKey.PoolType}"),
        };
    }
}
