using Chess2.Api.GameLogic;

namespace Chess2.Api.Game.Services;

public interface IGame
{
    public string Fen { get; }
}

public class Game : IGame
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );

    public string Fen { get; } = "";
}
