using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGame
{
    public string Fen { get; }
    IReadOnlyCollection<Move> Moves { get; }
    IReadOnlyCollection<Move> LegalMoves { get; }
}

public class Game : IGame
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );
    private readonly IFenCalculator _fenCalculator;
    private readonly List<Move> _legalMoves = [];

    public string Fen { get; }
    public IReadOnlyCollection<Move> Moves => _board.Moves;
    public IReadOnlyCollection<Move> LegalMoves => _legalMoves;

    public Game(IFenCalculator fenCalculator)
    {
        _fenCalculator = fenCalculator;
        Fen = _fenCalculator.CalculateFen(_board);
    }
}
