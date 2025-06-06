using Chess2.Api.GameLogic;

namespace Chess2.Api.Game.Models;

public interface IGameMessage
{
    public string GameToken { get; }
}

public class GameCommands
{
    public record StartGame(string GameToken, string UserId1, string UserId2) : IGameMessage;

    public record MovePiece(string GameToken, string UserId) : IGameMessage;

    public record Resign(string GameToken, string UserId) : IGameMessage;

    public record RequestDraw(string GameToken, string UserId) : IGameMessage;
}

public class GameQueries
{
    public record GetGameState(string GameToken) : IGameMessage;
}

public class GameEvents
{
    public record GameStateEvent(ChessBoard Board);
}
