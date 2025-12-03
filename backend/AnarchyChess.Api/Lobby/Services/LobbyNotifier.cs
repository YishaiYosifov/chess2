using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Lobby.SignalR;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Lobby.Services;

public interface ILobbyNotifier
{
    Task NotifyGameFoundAsync(
        UserId userId,
        IEnumerable<ConnectionId> connectionIds,
        OngoingGame game
    );
    Task NotifySeekFailedAsync(IEnumerable<ConnectionId> connectionIds, PoolKey pool);
    Task NotifyOngoingGamesAsync(ConnectionId connectionId, IEnumerable<OngoingGame> games);
    Task NotifyOngoingGameEndedAsync(UserId userId, GameToken gameToken);
}

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyHubClient> hub) : ILobbyNotifier
{
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub = hub;

    public async Task NotifyGameFoundAsync(
        UserId userId,
        IEnumerable<ConnectionId> connectionIds,
        OngoingGame game
    )
    {
        await _hub
            .Clients.Clients(connectionIds.Select(c => c.Value))
            .MatchFoundAsync(game.GameToken);
        await _hub.Clients.User(userId).ReceiveOngoingGamesAsync([game]);
    }

    public Task NotifySeekFailedAsync(IEnumerable<ConnectionId> connectionIds, PoolKey pool) =>
        _hub.Clients.Clients(connectionIds.Select(c => c.Value)).SeekFailedAsync(pool);

    public Task NotifyOngoingGamesAsync(
        ConnectionId connectionId,
        IEnumerable<OngoingGame> games
    ) => _hub.Clients.Client(connectionId).ReceiveOngoingGamesAsync(games);

    public Task NotifyOngoingGameEndedAsync(UserId userId, GameToken gameToken) =>
        _hub.Clients.User(userId).OngoingGameEndedAsync(gameToken);
}
