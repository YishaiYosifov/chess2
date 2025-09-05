using Chess2.Api.LiveGame.SignalR;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameChatNotifier
{
    Task JoinChatAsync(
        string gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    );
    Task LeaveChatAsync(
        string gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    );
    Task SendMessageAsync(
        string gameToken,
        string userName,
        ConnectionId connectionId,
        TimeSpan cooldownLeft,
        string message,
        bool isPlaying
    );
}

public class GameChatNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameChatNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    private static string GetGroupName(string gameToken, bool isPlaying) =>
        isPlaying ? $"{gameToken}:chat:playing" : $"{gameToken}:chat:spectators";

    public async Task JoinChatAsync(
        string gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    )
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.AddToGroupAsync(connectionId, groupName, token);
    }

    public async Task LeaveChatAsync(
        string gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    )
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.RemoveFromGroupAsync(connectionId, groupName, token);
    }

    public async Task SendMessageAsync(
        string gameToken,
        string userName,
        ConnectionId connectionId,
        TimeSpan cooldownLeft,
        string message,
        bool isPlaying
    )
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Clients.Group(groupName).ChatMessageAsync(userName, message);

        await _hub
            .Clients.Client(connectionId)
            .ChatMessageDeliveredAsync(cooldownLeft.TotalMilliseconds);
    }
}
