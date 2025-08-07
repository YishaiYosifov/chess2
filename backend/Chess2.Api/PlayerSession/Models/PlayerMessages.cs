namespace Chess2.Api.PlayerSession.Models;

public interface IPlayerSessionCommand
{
    public string UserId { get; }
}

public class PlayerSessionCommands
{
    public record CreateSeek(string UserId, string ConnectionId) : IPlayerSessionCommand;

    public record CancelSeek(string UserId, string ConnectionId) : IPlayerSessionCommand;

    public record GameEnded(string UserId, string GameToken) : IPlayerSessionCommand;
}

public class PlayerSessionReplies
{
    public record SeekCreated;

    public record SeekCanceled;

    public record MatchFound;
}
