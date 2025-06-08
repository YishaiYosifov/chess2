using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGame
{
    public string Fen { get; }
    IReadOnlyCollection<string> FenHistory { get; }
    public string EncodedLegalMoves { get; }

    void InitializeGame();
}

public class Game(
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    ILegalMoveEncoder legalMoveEncoder
) : IGame
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );
    private readonly List<string> _fenHistory = [];
    private List<Move> _legalMoves = [];

    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly ILegalMoveEncoder _legalMoveEncoder = legalMoveEncoder;

    public string Fen { get; private set; } = "";
    public string EncodedLegalMoves { get; private set; } = "";
    public IReadOnlyCollection<string> FenHistory => _fenHistory.AsReadOnly();
    public IReadOnlyCollection<Move> LegalMoves => _legalMoves.AsReadOnly();

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        _fenHistory.Add(Fen);
        _legalMoves = [.. _legalMoveCalculator.CalculateAllLegalMoves(_board)];
        EncodedLegalMoves = _legalMoveEncoder.EncodeLegalMoves(_legalMoves);
    }
}
