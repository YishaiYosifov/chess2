using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.ICasualMatchmakingGrain")]
public interface ICasualMatchmakingGrain : IMatchmakingGrain
{
    [Alias("CreateSeek")]
    Task CreateSeekAsync(string userId, IPlayerSessionGrain playerSessionGrain);
}

public class CasualMatchmakingGrain(
    ILogger<CasualMatchmakingGrain> logger,
    ILiveGameService liveGameService,
    IOptions<AppSettings> settings,
    ICasualMatchmakingPool pool
)
    : AbstractMatchmakinGrain<ICasualMatchmakingPool>(logger, liveGameService, settings, pool),
        ICasualMatchmakingGrain
{
    protected override bool IsRated => false;

    public Task CreateSeekAsync(string userId, IPlayerSessionGrain playerSessionGrain)
    {
        if (!TrySubscribeSeeker(userId, playerSessionGrain))
            return Task.CompletedTask;

        Logger.LogInformation("Received casual seek from {UserId}", userId);
        Pool.AddSeek(userId);
        return Task.CompletedTask;
    }
}
