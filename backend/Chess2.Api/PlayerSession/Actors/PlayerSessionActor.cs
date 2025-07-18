using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Hosting;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.PlayerSession.Models;

namespace Chess2.Api.PlayerSession.Actors;

public record ActiveSeekInfo(
    IActorRef PoolActor,
    TimeControlSettings TimeControl,
    string ConnectionId
);

public class PlayerSessionActor : ReceiveActor
{
    private readonly string _userId;
    private ActiveSeekInfo? _currentPool;
    private string? _gameToken;

    private readonly IRequiredActor<RatedMatchmakingActor> _ratedPoolActor;
    private readonly IRequiredActor<CasualMatchmakingActor> _casualPoolActor;
    private readonly IMatchmakingNotifier _matchmakingNotifier;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public PlayerSessionActor(
        string userId,
        IRequiredActor<RatedMatchmakingActor> ratedPoolActor,
        IRequiredActor<CasualMatchmakingActor> casualPoolActor,
        IMatchmakingNotifier matchmakingNotifier
    )
    {
        _userId = userId;
        _ratedPoolActor = ratedPoolActor;
        _casualPoolActor = casualPoolActor;
        _matchmakingNotifier = matchmakingNotifier;
        Become(Seeking);
    }

    private void Seeking()
    {
        Receive<PlayerSessionCommands.CreateSeek>(HandleCreateSeek);
        Receive<PlayerSessionCommands.CancelSeek>(HandleCancelSeek);
        Receive<MatchmakingEvents.MatchFound>(HandleMatchFound);
        Receive<MatchmakingEvents.MatchFailed>((_) => HandleMatchFailed());

        Receive<ReceiveTimeout>(_ =>
        {
            if (_currentPool is not null)
                return;

            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
            _logger.Info("Player is not seeking, passivating actor");
        });
    }

    private void HandleCreateSeek(PlayerSessionCommands.CreateSeek createSeek)
    {
        // Already seeking, cancel the previous seek
        _currentPool?.PoolActor.Tell(
            new MatchmakingCommands.CancelSeek(_userId, _currentPool.TimeControl)
        );

        var actor = ResolvePoolActorForSeek(createSeek.CreateSeekCommand);
        actor.Tell(createSeek.CreateSeekCommand);

        _currentPool = new ActiveSeekInfo(
            actor,
            createSeek.CreateSeekCommand.TimeControl,
            createSeek.ConnectionId
        );
    }

    private void HandleCancelSeek(PlayerSessionCommands.CancelSeek cancelSeek)
    {
        if (_currentPool is null)
            return;

        if (
            cancelSeek.ConnectionId is not null
            && cancelSeek.ConnectionId != _currentPool.ConnectionId
        )
        {
            _logger.Info(
                "User {0} attempted to cancel the seek of connection id {0}, but the seeking connection id is {1}",
                _userId,
                cancelSeek.ConnectionId,
                _currentPool.ConnectionId
            );
            return;
        }

        _currentPool.PoolActor.Tell(
            new MatchmakingCommands.CancelSeek(_userId, _currentPool.TimeControl)
        );
        _currentPool = null;
    }

    private void HandleMatchFound(MatchmakingEvents.MatchFound matchFound)
    {
        _gameToken = matchFound.GameToken;
        _currentPool = null;

        RunTask(() => _matchmakingNotifier.NotifyGameFoundAsync(_userId, matchFound.GameToken));

        Become(InGame);
    }

    private void HandleMatchFailed()
    {
        RunTask(() => _matchmakingNotifier.NotifyMatchFailedAsync(_userId));
    }

    private IActorRef ResolvePoolActorForSeek(ICreateSeekCommand createSeekCommand)
    {
        return createSeekCommand switch
        {
            RatedMatchmakingCommands.CreateRatedSeek _ => _ratedPoolActor.ActorRef,
            CasualMatchmakingCommands.CreateCasualSeek _ => _casualPoolActor.ActorRef,
            _ => throw new InvalidOperationException(
                $"Unsupported seek command type: {createSeekCommand.GetType()}"
            ),
        };
    }

    private void InGame()
    {
        if (_gameToken is null)
            throw new InvalidOperationException(
                $"Cannot transition to {nameof(InGame)} state when {nameof(_gameToken)} is not set"
            );

        Receive<PlayerSessionCommands.GameEnded>(_ => Become(Seeking));
        Receive<ReceiveTimeout>(_ => { });
    }

    protected override void PreStart()
    {
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
    }
}
