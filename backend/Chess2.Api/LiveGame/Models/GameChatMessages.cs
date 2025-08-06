namespace Chess2.Api.LiveGame.Models;

public interface IGameChatMessage
{
    public string GameToken { get; }
}

public class GameChatCommands
{
    public record JoinChat(string GameToken, string ConnectionId, string UserId) : IGameChatMessage;

    public record LeaveChat(string GameToken, string ConnectionId, string UserId)
        : IGameChatMessage;

    public record SendMessage(string GameToken, string ConnectionId, string UserId, string Message)
        : IGameChatMessage;
}

public class GameChatReplies
{
    public record UserJoined;

    public record UserLeft;

    public record MessageSent;
}
