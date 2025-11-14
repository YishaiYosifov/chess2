using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Lobby.SignalR;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Profile.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Lobby.Services;

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
