using Chess2.Api.LiveGame.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.LiveGame.Services;

public interface IGameChatNotifier
{
    Task JoinChat(string gameToken, string connectionId, bool isPlaying);
    Task LeaveChat(string gameToken, string connectionId, bool isPlaying);
    Task SendMessage(string gameToken, string userName, string message, bool isPlaying);
}

public class GameChatNotifier(IHubContext<GameHub, IGameHubClient> hub) : IGameChatNotifier
{
    private readonly IHubContext<GameHub, IGameHubClient> _hub = hub;

    private static string GetGroupName(string gameToken, bool isPlaying) =>
        isPlaying ? $"{gameToken}:chat:playing" : $"{gameToken}:chat:spectators";

    public async Task JoinChat(string gameToken, string connectionId, bool isPlaying)
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task LeaveChat(string gameToken, string connectionId, bool isPlaying)
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Groups.RemoveFromGroupAsync(connectionId, groupName);
    }

    public async Task SendMessage(string gameToken, string userName, string message, bool isPlaying)
    {
        var groupName = GetGroupName(gameToken, isPlaying);
        await _hub.Clients.Group(groupName).ChatMessageAsync(userName, message);
    }
}
