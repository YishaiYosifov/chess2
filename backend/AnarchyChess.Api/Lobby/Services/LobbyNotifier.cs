using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Lobby.SignalR;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Lobby.Services;

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
