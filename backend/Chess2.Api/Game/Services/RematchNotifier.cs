using Chess2.Api.Game.Models;
using Chess2.Api.Game.SignalR;
using Chess2.Api.Profile.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Game.Services;

public interface IRematchNotifier
{
    Task NotifyRematchAccepted(GameToken gameToken, UserId user1, UserId user2);
    Task NotifyRematchCancelledAsync(UserId user1, UserId user2);
    Task NotifyRematchRequestedAsync(UserId recipient);
}

public class RematchNotifier(IHubContext<GameHub, IGameHubClient> hub) : IRematchNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    public Task NotifyRematchRequestedAsync(UserId recipient) =>
        _hub.Clients.User(recipient).RematchRequestedAsync();

    public async Task NotifyRematchCancelledAsync(UserId user1, UserId user2)
    {
        await _hub.Clients.User(user1).RematchCancelledAsync();
        await _hub.Clients.User(user2).RematchCancelledAsync();
    }

    public async Task NotifyRematchAccepted(GameToken gameToken, UserId user1, UserId user2)
    {
        await _hub.Clients.User(user1).RematchAccepted(gameToken);
        await _hub.Clients.User(user2).RematchAccepted(gameToken);
    }
}
