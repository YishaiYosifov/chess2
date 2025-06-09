using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IGame
{
    string Fen { get; }
    IReadOnlyCollection<string> FenHistory { get; }

    void InitializeGame();
    IReadOnlyCollection<string> GetEncodedLegalMovesFor(GameColor forColor);
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
    private List<string> _encodedWhiteMoves = [];
    private List<string> _encodedBlackMoves = [];

    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly ILegalMoveEncoder _legalMoveEncoder = legalMoveEncoder;

    public string Fen { get; private set; } = "";

    public IReadOnlyCollection<string> FenHistory => _fenHistory.AsReadOnly();

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        _fenHistory.Add(Fen);
        _legalMoves = [.. _legalMoveCalculator.CalculateAllLegalMoves(_board)];

        _encodedWhiteMoves =
        [
            .. _legalMoveEncoder.EncodeLegalMoves(
                _legalMoves.Where(m => m.Piece.Color == GameColor.White)
            ),
        ];
        _encodedBlackMoves =
        [
            .. _legalMoveEncoder.EncodeLegalMoves(
                _legalMoves.Where(m => m.Piece.Color == GameColor.Black)
            ),
        ];
    }

    public IReadOnlyCollection<string> GetEncodedLegalMovesFor(GameColor forColor) =>
        forColor switch
        {
            GameColor.White => _encodedWhiteMoves.AsReadOnly(),
            GameColor.Black => _encodedBlackMoves.AsReadOnly(),
            _ => throw new InvalidOperationException($"Invalid Color {forColor}?"),
        };
}
