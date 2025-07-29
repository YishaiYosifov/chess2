using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Models;

public interface IGameMessage
{
    public string GameToken { get; }
}

public class GameCommands
{
    public record StartGame(
        string GameToken,
        GamePlayer WhitePlayer,
        GamePlayer BlackPlayer,
        TimeControlSettings TimeControl,
        bool IsRated
    ) : IGameMessage;

    public record TickClock;

    public record MovePiece(string GameToken, string UserId, AlgebraicPoint From, AlgebraicPoint To)
        : IGameMessage;

    public record EndGame(string GameToken, string UserId) : IGameMessage;

    public record RequestDraw(string GameToken, string UserId) : IGameMessage;
}

public class GameQueries
{
    public record GetGameState(string GameToken, string ForUserId) : IGameMessage;

    public record GetGamePlayers(string GameToken) : IGameMessage;

    public record IsGameOngoing(string GameToken) : IGameMessage;
}

public class GameResponses
{
    public record GameStarted;

    public record GameStateResponse(GameState State);

    public record GamePlayers(GamePlayer WhitePlayer, GamePlayer BlackPlayer);

    public record PieceMoved;

    public record GameEnded;
}
