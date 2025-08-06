using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Hosting;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.PlayerSession.Models;

namespace Chess2.Api.PlayerSession.Actors;

public class PlayerSessionActor : ReceiveActor
{
    private readonly string _userId;
    private readonly Dictionary<string, PoolKey> _connectionIdSeeks = [];
    private readonly Dictionary<PoolKey, HashSet<string>> _seekConnectionIds = [];
    private readonly HashSet<string> _activeGameTokens = [];

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

        Receive<PlayerSessionCommands.CreateSeek>(HandleCreateSeek);
        Receive<PlayerSessionCommands.CancelSeek>(HandleCancelSeek);
        Receive<MatchmakingEvents.MatchFound>(HandleMatchFound);
        Receive<MatchmakingEvents.MatchFailed>((_) => HandleMatchFailed());

        Receive<PlayerSessionCommands.GameEnded>(HandleGameEnd);

        Receive<ReceiveTimeout>(_ =>
        {
            if (_connectionIdSeeks.Count != 0 || _activeGameTokens.Count != 0)
                return;

            Context.Parent.Tell(new Passivate(PoisonPill.Instance));
            _logger.Info("Player is not seeking, passivating actor");
        });
    }

    private void HandleCreateSeek(PlayerSessionCommands.CreateSeek createSeek)
    {
        if (_connectionIdSeeks.TryGetValue(createSeek.ConnectionId, out var prevPoolKey))
        {
            // Already seeking, cancel the previous seek
            ResolvePoolActorForSeek(prevPoolKey.PoolType)
                .Tell(new MatchmakingCommands.CancelSeek(_userId, prevPoolKey));
        }

        var actor = ResolvePoolActorForSeek(createSeek.CreateSeekCommand.Key.PoolType);
        actor.Tell(createSeek.CreateSeekCommand);

        var poolKey = createSeek.CreateSeekCommand.Key;
        _connectionIdSeeks.Add(createSeek.ConnectionId, poolKey);

        var connections = _seekConnectionIds.GetValueOrDefault(poolKey, []);
        connections.Add(createSeek.ConnectionId);
        _seekConnectionIds[poolKey] = connections;

        Sender.Tell(new PlayerSessionReplies.SeekCreated());
    }

    private void HandleCancelSeek(PlayerSessionCommands.CancelSeek cancelSeek)
    {
        if (!_connectionIdSeeks.TryGetValue(cancelSeek.ConnectionId, out var poolKey))
        {
            _logger.Info(
                "User {0} attempted to cancel seek with connection id {1}, but no seek was found",
                _userId,
                cancelSeek.ConnectionId
            );
            return;
        }

        var actor = ResolvePoolActorForSeek(poolKey.PoolType);
        actor.Tell(new MatchmakingCommands.CancelSeek(_userId, poolKey));
        _connectionIdSeeks.Remove(cancelSeek.ConnectionId);

        var seekConnections = _seekConnectionIds.GetValueOrDefault(poolKey, []);
        seekConnections.Remove(cancelSeek.ConnectionId);
        if (seekConnections.Count == 0)
            _seekConnectionIds.Remove(poolKey);
        else
            _seekConnectionIds[poolKey] = seekConnections;

        Sender.Tell(new PlayerSessionReplies.SeekCanceled());
    }

    private void HandleMatchFound(MatchmakingEvents.MatchFound matchFound)
    {
        _activeGameTokens.Add(matchFound.GameToken);
        var poolKey = matchFound.Key;
        var connectionIds = _seekConnectionIds.GetValueOrDefault(poolKey, []);

        foreach (var connectionId in connectionIds)
        {
            RunTask(
                () => _matchmakingNotifier.NotifyGameFoundAsync(connectionId, matchFound.GameToken)
            );
            _connectionIdSeeks.Remove(connectionId);
        }
        _seekConnectionIds.Remove(poolKey);

        Sender.Tell(new PlayerSessionReplies.MatchFound());
    }

    private void HandleMatchFailed() =>
        RunTask(() => _matchmakingNotifier.NotifyMatchFailedAsync(_userId));

    private void HandleGameEnd(PlayerSessionCommands.GameEnded gameEnded) =>
        _activeGameTokens.Remove(gameEnded.GameToken);

    private IActorRef ResolvePoolActorForSeek(PoolType poolType)
    {
        return poolType switch
        {
            PoolType.Rated => _ratedPoolActor.ActorRef,
            PoolType.Casual => _casualPoolActor.ActorRef,
            _ => throw new InvalidOperationException($"Unsupported pool type: {poolType}"),
        };
    }

    protected override void PreStart() => Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
}
