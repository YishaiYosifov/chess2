using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Hubs;

public interface IMatchmakingClient
{
    public Task ReceiveMessage(string message);
}

[Authorize(Policy = "AccessToken")]
[Authorize(Policy = "GuestToken")]
public class MatchmakingHub : Hub<IMatchmakingClient>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.ReceiveMessage($"{Context.ConnectionId} has joined");
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.ReceiveMessage($"{Context.ConnectionId}: {message}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.ReceiveMessage($"{Context.ConnectionId} has left");
    }
}
