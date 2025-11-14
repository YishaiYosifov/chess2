using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.SignalR;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Game.Services;

public interface IGameChatNotifier
{
    Task JoinChatAsync(
        GameToken gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    );
    Task LeaveChatAsync(
        GameToken gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    );
    Task SendMessageAsync(
        GameToken gameToken,
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

    private static string GetGroupName(GameToken gameToken, bool isPlaying) =>
        isPlaying ? $"{gameToken}:chat:playing" : $"{gameToken}:chat:spectators";

    public async Task JoinChatAsync(
        GameToken gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    )
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.AddToGroupAsync(connectionId, groupName, token);
    }

    public async Task LeaveChatAsync(
        GameToken gameToken,
        ConnectionId connectionId,
        bool isPlaying,
        CancellationToken token = default
    )
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.RemoveFromGroupAsync(connectionId, groupName, token);
    }

    public async Task SendMessageAsync(
        GameToken gameToken,
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
