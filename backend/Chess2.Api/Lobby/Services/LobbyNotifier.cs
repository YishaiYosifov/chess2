using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Lobby.Services;

public interface ILobbyNotifier
{
    Task NotifyGameFoundAsync(IEnumerable<ConnectionId> connectionIds, string gameToken);
    Task NotifyMatchFailedAsync(IEnumerable<ConnectionId> connectionIds);
}

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyHubClient> hub) : ILobbyNotifier
{
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(IEnumerable<ConnectionId> connectionIds, string gameToken) =>
        _hub.Clients.Clients(connectionIds.Select(c => c.Value)).MatchFoundAsync(gameToken);

    public Task NotifyMatchFailedAsync(IEnumerable<ConnectionId> connectionIds) =>
        _hub.Clients.Clients(connectionIds.Select(c => c.Value)).MatchFailedAsync();
}
