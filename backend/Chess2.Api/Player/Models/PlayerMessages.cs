using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Player.Models;

public interface IPlayerCommand
{
    public string UserId { get; }
}

public class PlayerCommands
{
    public record CreateSeek(
        string UserId,
        string ConnectionId,
        ICreateSeekCommand CreateSeekCommand
    ) : IPlayerCommand;

    public record CancelSeek(string UserId, string? ConnectionId) : IPlayerCommand;
}
