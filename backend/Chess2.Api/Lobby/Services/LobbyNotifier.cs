using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Lobby.Services;

public interface ILobbyNotifier
{
    Task NotifyGameFoundAsync(ConnectionId connectionId, string gameToken);
    Task NotifyMatchFailedAsync(ConnectionId connectionId);
}

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyHubClient> hub) : ILobbyNotifier
{
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(ConnectionId connectionId, string gameToken) =>
        _hub.Clients.Client(connectionId).MatchFoundAsync(gameToken);

    public Task NotifyMatchFailedAsync(ConnectionId connectionId) =>
        _hub.Clients.Client(connectionId).MatchFailedAsync();
}
