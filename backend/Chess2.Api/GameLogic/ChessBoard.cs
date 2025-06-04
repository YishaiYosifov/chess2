using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Errors;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.GameLogic;

public class ChessBoard
{
    private readonly Piece?[,] _board = new Piece?[10, 10];

    private readonly List<Move> _moves = [];

    public IReadOnlyCollection<Move> Moves => _moves;
    public Move? LastMove => _moves.Count > 0 ? _moves[0] : null;

    public ChessBoard(Dictionary<Point, Piece> pieces)
    {
        InitializeBoard(pieces);
    }

    private void InitializeBoard(Dictionary<Point, Piece> pieces)
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                _board[y, x] = pieces.GetValueOrDefault(new Point(x, y));
            }
        }
    }

    public bool TryGetPieceAt(Point point, [NotNullWhen(true)] out Piece? piece)
    {
        piece = _board[point.Y, point.X];
        return piece is not null;
    }

    public Piece? PeekPieceAt(Point point) => _board[point.Y, point.X];

    public bool IsEmpty(Point point) => _board[point.Y, point.X] is null;

    public ErrorOr<Success> MovePiece(Point from, Point to)
    {
        if (!TryGetPieceAt(from, out var piece))
            return GameErrors.PieceNotFound;

        _board[from.X, from.Y] = null;
        _board[to.X, to.Y] = piece with { TimesMoved = piece.TimesMoved + 1 };
        return Result.Success;
    }

    public bool IsWithinBoundaries(Point point) =>
        point.Y >= 0
        && point.Y < _board.GetLength(0)
        && point.X >= 0
        && point.X < _board.GetLength(1);

    public IEnumerable<(Point Position, Piece? Piece)> GetSquares()
    {
        for (int y = 0; y < _board.GetLength(0); y++)
        {
            for (int x = 0; x < _board.GetLength(1); x++)
            {
                yield return (new Point(x, y), _board[y, x]);
            }
        }
    }
}
