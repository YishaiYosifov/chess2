using Chess2.Api.Lobby.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingNotifier
{
    Task NotifyGameFoundAsync(string connectionId, string gameToken);
    Task NotifyMatchFailedAsync(string connectionId);
}

public class MatchmakingNotifier(IHubContext<LobbyHub, IMatchmakingHubClient> hub)
    : IMatchmakingNotifier
{
    private readonly IHubContext<LobbyHub, IMatchmakingHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(string connectionId, string gameToken) =>
        _hub.Clients.Client(connectionId).MatchFoundAsync(gameToken);

    public Task NotifyMatchFailedAsync(string connectionId) =>
        _hub.Clients.Client(connectionId).MatchFailedAsync();
}
