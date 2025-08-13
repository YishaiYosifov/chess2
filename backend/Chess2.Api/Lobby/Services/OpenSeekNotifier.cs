using Chess2.Api.Lobby.Grains;
using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Lobby.Services;

public interface IOpenSeekNotifier
{
    Task NotifyOpenSeekAsync(IEnumerable<string> userIds, IEnumerable<OpenSeek> openSeeks);
    Task NotifyOpenSeekAsync(ConnectionId connectionId, IEnumerable<OpenSeek> openSeeks);
    Task NotifyOpenSeekEndedAsync(IEnumerable<string> userIds, SeekKey seekKey);
}

public class OpenSeekNotifier(IHubContext<OpenSeekHub, IOpenSeekHubClient> hub) : IOpenSeekNotifier
{
    private readonly IHubContext<OpenSeekHub, IOpenSeekHubClient> _hub = hub;

    public Task NotifyOpenSeekAsync(IEnumerable<string> userIds, IEnumerable<OpenSeek> openSeeks) =>
        _hub.Clients.Users(userIds).NewOpenSeekAsync(openSeeks);

    public Task NotifyOpenSeekAsync(ConnectionId connectionId, IEnumerable<OpenSeek> openSeeks) =>
        _hub.Clients.Client(connectionId).NewOpenSeekAsync(openSeeks);

    public Task NotifyOpenSeekEndedAsync(IEnumerable<string> userIds, SeekKey seekKey) =>
        _hub.Clients.Users(userIds).OpenSeekEndedAsync(seekKey);
}
