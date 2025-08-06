using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;

namespace Chess2.Api.PlayerSession.Actors;

[Alias("Chess2.Api.PlayerSession.Actors.IPlayerSessionGrain")]
public interface IPlayerSessionGrain : IGrainWithStringKey
{
    [Alias("RegisterSeekAsync")]
    Task RegisterSeekAsync(string connectionId, PoolKey poolKey);

    [Alias("RemoveSeeksForConnectionAsync")]
    Task<PoolKey?> RemoveSeekForConnectionAsync(string connectionId);

    [Alias("MatchFoundAsync")]
    Task MatchFoundAsync(string gameToken, PoolKey poolKey);

    [Alias("MatchFailedAsync")]
    Task MatchFailedAsync(PoolKey poolKey);

    [Alias("GameEndedAsync")]
    Task GameEndedAsync(string gameToken);
}

public class PlayerSessionGrain : Grain, IPlayerSessionGrain, IGrainBase
{
    private readonly string _userId;
    private readonly Dictionary<string, PoolKey> _connectionIdSeeks = [];
    private readonly Dictionary<PoolKey, HashSet<string>> _seekConnectionIds = [];
    private readonly HashSet<string> _activeGameTokens = [];
    private readonly ILogger<PlayerSessionGrain> _logger;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public PlayerSessionGrain(
        ILogger<PlayerSessionGrain> logger,
        IMatchmakingNotifier matchmakingNotifier
    )
    {
        _userId = this.GetPrimaryKeyString();

        _logger = logger;
        _matchmakingNotifier = matchmakingNotifier;
    }

    public Task RegisterSeekAsync(string connectionId, PoolKey poolKey)
    {
        _connectionIdSeeks.Add(connectionId, poolKey);

        var connections = _seekConnectionIds.GetValueOrDefault(poolKey, []);
        connections.Add(connectionId);
        _seekConnectionIds[poolKey] = connections;

        return Task.CompletedTask;
    }

    public Task<PoolKey?> RemoveSeekForConnectionAsync(string connectionId)
    {
        if (!_connectionIdSeeks.TryGetValue(connectionId, out var poolKey))
        {
            _logger.LogInformation(
                "User {UserId} attempted to cancel seek with connection id {ConnectionId}, but no seek was found",
                _userId,
                connectionId
            );
            return Task.FromResult<PoolKey?>(null);
        }

        _connectionIdSeeks.Remove(connectionId);

        var seekConnections = _seekConnectionIds.GetValueOrDefault(poolKey, []);
        seekConnections.Remove(connectionId);
        if (seekConnections.Count == 0)
            _seekConnectionIds.Remove(poolKey);
        else
            _seekConnectionIds[poolKey] = seekConnections;

        return Task.FromResult<PoolKey?>(poolKey);
    }

    public async Task MatchFoundAsync(string gameToken, PoolKey poolKey)
    {
        _activeGameTokens.Add(gameToken);

        var connectionIds = _seekConnectionIds.GetValueOrDefault(poolKey, []);
        foreach (var connectionId in connectionIds)
        {
            await _matchmakingNotifier.NotifyGameFoundAsync(connectionId, gameToken);
        }
        RemoveSeek(poolKey);
    }

    public async Task MatchFailedAsync(PoolKey poolKey)
    {
        var connectionIds = _seekConnectionIds.GetValueOrDefault(poolKey, []);
        foreach (var connectionId in connectionIds)
        {
            await _matchmakingNotifier.NotifyMatchFailedAsync(connectionId);
        }
        RemoveSeek(poolKey);
    }

    public Task GameEndedAsync(string gameToken) =>
        Task.FromResult(_activeGameTokens.Remove(gameToken));

    private void RemoveSeek(PoolKey poolKey)
    {
        var connectionIds = _seekConnectionIds.GetValueOrDefault(poolKey, []);

        foreach (var connectionId in connectionIds)
            _connectionIdSeeks.Remove(connectionId);
        _seekConnectionIds.Remove(poolKey);
    }
}
