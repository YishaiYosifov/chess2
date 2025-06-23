using Akka.Actor;
using Akka.Event;
using Chess2.Api.Game.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public class CasualMatchmakingActor(
    string entityId,
    IOptions<AppSettings> settings,
    ICasualMatchmakingPool pool,
    IGameService gameService,
    ITimerScheduler? timerScheduler = null
)
    : AbstractMatchmakingActor<ICasualMatchmakingPool>(
        entityId,
        settings,
        pool,
        gameService,
        timerScheduler
    )
{
    protected override bool EnterPool(ICreateSeekCommand createSeek)
    {
        if (createSeek is not CasualMatchmakingCommands.CreateCasualSeek createCasualSeek)
        {
            Unhandled(createSeek);
            return false;
        }

        Logger.Info("Received casual seek from {0}", createSeek.UserId);
        Pool.AddSeek(createCasualSeek.UserId);
        return true;
    }
}
