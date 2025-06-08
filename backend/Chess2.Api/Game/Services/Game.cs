using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGame
{
    public string Fen { get; }
    IReadOnlyCollection<string> FenHistory { get; }
    IReadOnlyCollection<Move> LegalMoves { get; }
}

public class Game : IGame
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );
    private readonly List<Move> _legalMoves = [];
    private readonly List<string> _fenHistory = [];

    private readonly IFenCalculator _fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator;

    public string Fen { get; private set; }
    public IReadOnlyCollection<string> FenHistory => _fenHistory.AsReadOnly();
    public IReadOnlyCollection<Move> LegalMoves => _legalMoves.AsReadOnly();

    public Game(IFenCalculator fenCalculator, ILegalMoveCalculator legalMoveCalculator)
    {
        _fenCalculator = fenCalculator;
        _legalMoveCalculator = legalMoveCalculator;

        Fen = _fenCalculator.CalculateFen(_board);
        _fenHistory.Add(Fen);
        _legalMoves = [.. _legalMoveCalculator.CalculateAllLegalMoves(_board)];
    }
}
