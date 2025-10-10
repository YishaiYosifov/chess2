using Chess2.Api.LiveGame.Models;
using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Lobby.Services;

public interface ILobbyNotifier
{
    Task NotifyGameFoundAsync(IEnumerable<ConnectionId> connectionIds, GameToken gameToken);
    Task NotifySeekFailedAsync(IEnumerable<ConnectionId> connectionIds, PoolKey pool);
}

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyHubClient> hub) : ILobbyNotifier
{
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(
        IEnumerable<ConnectionId> connectionIds,
        GameToken gameToken
    ) => _hub.Clients.Clients(connectionIds.Select(c => c.Value)).MatchFoundAsync(gameToken);

    public Task NotifySeekFailedAsync(IEnumerable<ConnectionId> connectionIds, PoolKey pool) =>
        _hub.Clients.Clients(connectionIds.Select(c => c.Value)).SeekFailedAsync(pool);
}
