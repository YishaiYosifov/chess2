using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.PlayerSession.Grains;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekRatedAsync(AuthedUser user, string connectionId, TimeControlSettings timeControl);

    Task SeekCasualAsync(
        string userId,
        string connectionId,
        AuthedUser? user,
        TimeControlSettings timeControl
    );

    Task CancelSeekAsync(string userId, string connectionId);
}

public class MatchmakingService(
    IGrainFactory grains,
    IRatingService ratingService,
    ITimeControlTranslator secondsToTimeControl,
    IOptions<AppSettings> settings
) : IMatchmakingService
{
    private readonly IGrainFactory _grains = grains;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;
    private readonly GameSettings _settings = settings.Value.Game;

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
        RatedSeeker seeker = new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: [],
            Rating: new(Value: rating, AllowedRatingRange: _settings.AllowedMatchRatingDifference)
        );

        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(user.Id);
        await playerSessionGrain.CreateSeekAsync(connectionId, seeker, poolKey);
    }

    public async Task SeekCasualAsync(
        string userId,
        string connectionId,
        AuthedUser? user,
        TimeControlSettings timeControl
    )
    {
        PoolKey poolKey = new(PoolType.Casual, timeControl);
        Seeker seeker = new(
            UserId: userId,
            UserName: user?.UserName ?? "Guest",
            BlockedUserIds: []
        );

        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        await playerSessionGrain.CreateSeekAsync(connectionId, seeker, poolKey);
    }

    public async Task CancelSeekAsync(string userId, string connectionId)
    {
        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        await playerSessionGrain.CancelSeekAsync(connectionId);
    }
}
