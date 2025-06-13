using Chess2.Api.Game.Errors;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGame
{
    string Fen { get; }
    IReadOnlyCollection<string> EncodedMoveHistory { get; }
    int MoveNumber { get; }

    void InitializeGame();
    IReadOnlyCollection<string> GetEncodedLegalMovesFor(GameColor forColor);
    ErrorOr<string> MakeMove(Point from, Point to);
}

public class Game(
    ILogger<Game> logger,
    IFenCalculator fenCalculator,
    ILegalMoveCalculator legalMoveCalculator,
    IMoveEncoder legalMoveEncoder
) : IGame
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );
    private readonly List<string> _encodedMoveHistory = [];

    private Dictionary<(Point from, Point to), Move> _legalMoves = [];
    private List<string> _encodedWhiteMoves = [];
    private List<string> _encodedBlackMoves = [];

    private readonly ILogger<Game> _logger = logger;
    private readonly IFenCalculator _fenCalculator = fenCalculator;
    private readonly ILegalMoveCalculator _legalMoveCalculator = legalMoveCalculator;
    private readonly IMoveEncoder _moveEncoder = legalMoveEncoder;

    public string Fen { get; private set; } = "";
    public IReadOnlyCollection<string> EncodedMoveHistory => _encodedMoveHistory.AsReadOnly();
    public int MoveNumber => EncodedMoveHistory.Count;

    public void InitializeGame()
    {
        Fen = _fenCalculator.CalculateFen(_board);
        CalculateAllLegalMoves();
    }

    public IReadOnlyCollection<string> GetEncodedLegalMovesFor(GameColor forColor) =>
        forColor switch
        {
            GameColor.White => _encodedWhiteMoves.AsReadOnly(),
            GameColor.Black => _encodedBlackMoves.AsReadOnly(),
            _ => throw new InvalidOperationException($"Invalid Color {forColor}?"),
        };

    public ErrorOr<string> MakeMove(Point from, Point to)
    {
        if (!_legalMoves.TryGetValue((from, to), out var move))
        {
            _logger.LogWarning("Could not find move from {From} to {To}", from, to);
            return GameErrors.MoveInvalid;
        }

        MakeMoveOnBoard(move);

        CalculateAllLegalMoves();
        Fen = _fenCalculator.CalculateFen(_board);

        var encodedMove = _moveEncoder.EncodeSingleMove(move);
        _encodedMoveHistory.Add(encodedMove);
        return encodedMove;
    }

    private void MakeMoveOnBoard(Move move)
    {
        foreach (var capture in move.CapturedSquares ?? [])
        {
            _board.ClearSquare(capture);
        }

        _board.MovePiece(move.From, move.To);

        foreach (var sideEffect in move.SideEffects ?? [])
        {
            MakeMoveOnBoard(sideEffect);
        }
    }

    private void CalculateAllLegalMoves()
    {
        var legalMoves = _legalMoveCalculator.CalculateAllLegalMoves(_board).ToList();

        _encodedWhiteMoves =
        [
            .. _moveEncoder.EncodeMoves(legalMoves.Where(m => m.Piece.Color == GameColor.White)),
        ];
        _encodedBlackMoves =
        [
            .. _moveEncoder.EncodeMoves(legalMoves.Where(m => m.Piece.Color == GameColor.Black)),
        ];

        _legalMoves = [];
        foreach (var move in legalMoves)
        {
            var key = (move.From, move.To);
            if (!_legalMoves.TryAdd(key, move))
            {
                _logger.LogWarning("Duplicate move found from {From} to {To}", move.From, move.To);
                continue;
            }
        }
    }
}
