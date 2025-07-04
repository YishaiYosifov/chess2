using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.PlayerSession.Models;

public interface IPlayerSessionCommand
{
    public string UserId { get; }
}

public class PlayerSessionCommands
{
    public record CreateSeek(
        string UserId,
        string ConnectionId,
        ICreateSeekCommand CreateSeekCommand
    ) : IPlayerSessionCommand;

    public record CancelSeek(string UserId, string? ConnectionId = null) : IPlayerSessionCommand;

    public record GameEnded(string UserId) : IPlayerSessionCommand;
}
