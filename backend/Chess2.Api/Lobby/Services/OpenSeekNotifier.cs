using Chess2.Api.Lobby.Models;
using Chess2.Api.Lobby.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Lobby.Services;

public interface IOpenSeekNotifier
{
    Task NotifyOpenSeekAsync(IEnumerable<string> userIds, IEnumerable<OpenSeek> openSeeks);
    Task NotifyOpenSeekAsync(ConnectionId connectionId, IEnumerable<OpenSeek> openSeeks);
    Task NotifyOpenSeekEndedAsync(IEnumerable<string> userIds, UserId seekerId, PoolKey pool);
}

public class OpenSeekNotifier(IHubContext<OpenSeekHub, IOpenSeekHubClient> hub) : IOpenSeekNotifier
{
    private readonly IHubContext<OpenSeekHub, IOpenSeekHubClient> _hub = hub;

    public Task NotifyOpenSeekAsync(IEnumerable<string> userIds, IEnumerable<OpenSeek> openSeeks) =>
        _hub.Clients.Users(userIds).NewOpenSeeksAsync(openSeeks);

    public Task NotifyOpenSeekAsync(ConnectionId connectionId, IEnumerable<OpenSeek> openSeeks) =>
        _hub.Clients.Client(connectionId).NewOpenSeeksAsync(openSeeks);

    public Task NotifyOpenSeekEndedAsync(
        IEnumerable<string> userIds,
        UserId seekerId,
        PoolKey pool
    ) => _hub.Clients.Users(userIds).OpenSeekEndedAsync(seekerId, pool);
}
