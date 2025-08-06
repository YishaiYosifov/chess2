using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IRatedMatchmakingGrain")]
public interface IRatedMatchmakingGrain : IMatchmakingGrain
{
    [Alias("CreateSeek")]
    Task CreateSeekAsync(string userId, int rating, IPlayerSessionGrain playerSessionGrain);
}

public class RatedMatchmakingGrain(
    ILogger<RatedMatchmakingGrain> logger,
    ILiveGameService liveGameService,
    IOptions<AppSettings> settings,
    IRatedMatchmakingPool pool
)
    : AbstractMatchmakinGrain<IRatedMatchmakingPool>(logger, liveGameService, settings, pool),
        IRatedMatchmakingGrain
{
    protected override bool IsRated => true;

    public Task CreateSeekAsync(string userId, int rating, IPlayerSessionGrain playerSessionGrain)
    {
        if (!TrySubscribeSeeker(userId, playerSessionGrain))
            return Task.CompletedTask;

        Logger.LogInformation("Received rated seek from {UserId}", userId);
        Pool.AddSeek(userId, rating);
        return Task.CompletedTask;
    }
}
