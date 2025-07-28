using Chess2.Api.LiveGame.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameChatNotifier
{
    Task JoinChatAsync(string gameToken, string connectionId, bool isPlaying);
    Task LeaveChatAsync(string gameToken, string connectionId, bool isPlaying);
    Task SendMessageAsync(
        string gameToken,
        string userName,
        string connectionId,
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

    public async Task JoinChatAsync(string gameToken, string connectionId, bool isPlaying)
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task LeaveChatAsync(string gameToken, string connectionId, bool isPlaying)
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.RemoveFromGroupAsync(connectionId, groupName);
    }

    public async Task SendMessageAsync(
        string gameToken,
        string userName,
        string connectionId,
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
