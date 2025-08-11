using Chess2.Api.Lobby.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Matchmaking.Services;

public interface ILobbyNotifier
{
    Task NotifyGameFoundAsync(string connectionId, string gameToken);
    Task NotifyMatchFailedAsync(string connectionId);
}

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyHubClient> hub) : ILobbyNotifier
{
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(string connectionId, string gameToken) =>
        _hub.Clients.Client(connectionId).MatchFoundAsync(gameToken);

    public Task NotifyMatchFailedAsync(string connectionId) =>
        _hub.Clients.Client(connectionId).MatchFailedAsync();
}
