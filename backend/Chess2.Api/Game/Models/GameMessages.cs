using Chess2.Api.Game.DTOs;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Models;

public interface IGameMessage
{
    public string GameToken { get; }
}

public enum GameStatus
{
    NotStarted,
    OnGoing,
}

public class GameCommands
{
    public record StartGame(string GameToken, string WhiteId, string BlackId) : IGameMessage;

    public record MovePiece(string GameToken, string UserId, AlgebraicPoint From, AlgebraicPoint To)
        : IGameMessage;

    public record Resign(string GameToken, string UserId) : IGameMessage;

    public record RequestDraw(string GameToken, string UserId) : IGameMessage;
}

public class GameQueries
{
    public record GetGameState(string GameToken, string ForUserId) : IGameMessage;

    public record GetGameStatus(string GameToken) : IGameMessage;
}

public class GameEvents
{
    public record GameStartedEvent();

    public record GameStatusEvent(GameStatus Status);

    public record GameStateEvent(GameStateDto State);

    public record PieceMoved(
        string Move,
        IReadOnlyCollection<string> WhiteLegalMoves,
        string WhiteId,
        IReadOnlyCollection<string> BlackLegalMoves,
        string BlackId,
        GameColor SideToMove,
        int MoveNumber
    );
}
