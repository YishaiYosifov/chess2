using Akka.Actor;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public class RatedMatchmakingActor(
    string entityId,
    IServiceProvider sp,
    IOptions<AppSettings> settings,
    IRatedMatchmakingPool pool,
    ITimerScheduler? timerScheduler = null
) : AbstractMatchmakingActor<IRatedMatchmakingPool>(entityId, sp, settings, pool, timerScheduler)
{
    protected override bool EnterPool(ICreateSeekCommand createSeek)
    {
        if (createSeek is not RatedMatchmakingCommands.CreateRatedSeek createCasualSeek)
        {
            Unhandled(createSeek);
            return false;
        }

        Logger.Info(
            "Received seek from {0} with rating {1}",
            createCasualSeek.UserId,
            createCasualSeek.Rating
        );
        Pool.AddSeek(createSeek.UserId, createCasualSeek.Rating);
        return true;
    }
}
