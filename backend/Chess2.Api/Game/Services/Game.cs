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
    private readonly IFenCalculator _fenCalculator;

    public string Fen { get; }

    public Game(IFenCalculator fenCalculator)
    {
        _fenCalculator = fenCalculator;
        Fen = _fenCalculator.CalculateFen(_board);
    }
}
