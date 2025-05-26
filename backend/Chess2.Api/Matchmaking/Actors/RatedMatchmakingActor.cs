using Akka.Actor;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Actors;

public class RatedMatchmakingActor : AbstractMatchmakingActor<IRatedMatchmakingPool>
{
    public RatedMatchmakingActor(
        IOptions<AppSettings> settings,
        IRatedMatchmakingPool pool,
        ITimerScheduler? timerScheduler = null
    )
        : base(settings, pool, timerScheduler)
    {
        Receive<MatchmakingCommands.CreateRatedSeek>(HandleCreateSeek);
    }

    private void HandleCreateSeek(MatchmakingCommands.CreateRatedSeek createSeek)
    {
        Logger.Info("Received seek from {0} with rating {1}", createSeek.UserId, createSeek.Rating);

        Pool.AddSeek(createSeek.UserId, createSeek.Rating);
    }
}
