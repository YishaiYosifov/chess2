using Akka.Actor;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public class CasualMatchmakingActor : AbstractMatchmakingActor<ICasualMatchmakingPool>
{
    public CasualMatchmakingActor(
        IOptions<AppSettings> settings,
        ICasualMatchmakingPool pool,
        ITimerScheduler? timerScheduler = null
    )
        : base(settings, pool, timerScheduler)
    {
        Receive<MatchmakingCommands.CreateCasualSeek>(HandleCreateSeek);
    }

    private void HandleCreateSeek(MatchmakingCommands.CreateCasualSeek createSeek)
    {
        Logger.Info("Received casual seek from {0}", createSeek.UserId);

        Pool.AddSeek(createSeek.UserId);
    }
}
