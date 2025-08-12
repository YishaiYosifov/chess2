using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.PlayerSession.Grains;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Matchmaking.Services;

public interface ILobbyNotifier
{
    Task NotifyGameFoundAsync(ConnectionId connectionId, string gameToken);
    Task NotifyMatchFailedAsync(ConnectionId connectionId);
    Task NotifyOpenSeekAsync(IEnumerable<string> connectionIds, IEnumerable<OpenSeek> openSeeks);
    Task NotifyOpenSeekEndedAsync(IEnumerable<string> connectionIds, SeekKey seekKey);
}

public class LobbyNotifier(IHubContext<LobbyHub, ILobbyHubClient> hub) : ILobbyNotifier
{
    private readonly IHubContext<LobbyHub, ILobbyHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(ConnectionId connectionId, string gameToken) =>
        _hub.Clients.Client(connectionId).MatchFoundAsync(gameToken);

    public Task NotifyMatchFailedAsync(ConnectionId connectionId) =>
        _hub.Clients.Client(connectionId).MatchFailedAsync();

    public Task NotifyOpenSeekAsync(IEnumerable<string> userIds, IEnumerable<OpenSeek> openSeeks) =>
        _hub.Clients.Users(userIds).NewOpenSeekAsync(openSeeks);

    public Task NotifyOpenSeekEndedAsync(IEnumerable<string> userIds, SeekKey seekKey) =>
        _hub.Clients.Users(userIds).OpenSeekEndedAsync(seekKey);
}
