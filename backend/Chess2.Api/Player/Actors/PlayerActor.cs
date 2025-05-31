using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.Player.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Player.Actors;

public record ActiveSeekInfo(IActorRef PoolActor, PoolInfo PoolInfo, string ConnectionId);

public class PlayerActor : ReceiveActor
{
    private readonly string _userId;
    private ActiveSeekInfo? _currentPool;
    private string? _gameId;

    private readonly IRequiredActor<RatedMatchmakingActor> _ratedPoolActor;
    private readonly IRequiredActor<CasualMatchmakingActor> _casualPoolActor;
    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _matchmakingHubContext;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public PlayerActor(
        string userId,
        IRequiredActor<RatedMatchmakingActor> ratedPoolActor,
        IRequiredActor<CasualMatchmakingActor> casualPoolActor,
        IHubContext<MatchmakingHub, IMatchmakingClient> matchmakingHubContext
    )
    {
        _userId = userId;
        _ratedPoolActor = ratedPoolActor;
        _casualPoolActor = casualPoolActor;
        _matchmakingHubContext = matchmakingHubContext;
        Become(Seeking);
    }

    private void Seeking()
    {
        Receive<PlayerCommands.CreateSeek>(createSeek =>
        {
            // Already seeking, cancel the previous seek
            _currentPool?.PoolActor.Tell(
                new MatchmakingCommands.CancelSeek(_userId, _currentPool.PoolInfo)
            );

            var actor = ResolvePoolActorForSeek(createSeek.CreateSeekCommand);
            actor.Tell(createSeek.CreateSeekCommand);

            _currentPool = new ActiveSeekInfo(
                actor,
                createSeek.CreateSeekCommand.PoolInfo,
                createSeek.ConnectionId
            );
        });

        Receive<PlayerCommands.CancelSeek>(cancelSeek =>
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
                new MatchmakingCommands.CancelSeek(_userId, _currentPool.PoolInfo)
            );
            _currentPool = null;
        });

        Receive<MatchmakingEvents.MatchFound>(matchFound =>
        {
            _gameId = matchFound.GameId;
            _currentPool = null;

            RunTask(
                () =>
                    _matchmakingHubContext.Clients.User(_userId).MatchFoundAsync(matchFound.GameId)
            );

            Become(InGame);
        });
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
        if (_gameId is null)
            throw new InvalidOperationException(
                $"Cannot transition to {nameof(InGame)} state when {nameof(_gameId)} is not set"
            );
    }
}
