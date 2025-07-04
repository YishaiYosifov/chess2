using Chess2.Api.Matchmaking.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingNotifier
{
    Task NotifyGameFoundAsync(string userId, string gameToken);
    Task NotifyMatchFailedAsync(string userId);
}

public class MatchmakingNotifier(IHubContext<MatchmakingHub, IMatchmakingHubClient> hub)
    : IMatchmakingNotifier
{
    private readonly IHubContext<MatchmakingHub, IMatchmakingHubClient> _hub = hub;

    public Task NotifyGameFoundAsync(string userId, string gameToken) =>
        _hub.Clients.User(userId).MatchFoundAsync(gameToken);

    public Task NotifyMatchFailedAsync(string userId) =>
        _hub.Clients.User(userId).MatchFailedAsync();
}
