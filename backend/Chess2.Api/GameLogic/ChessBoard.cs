using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Game;
using Chess2.Api.GameLogic.Errors;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.GameLogic;

public class ChessBoard
{
    private readonly Piece?[,] _board;

    private readonly List<Move> _moves = [];

    public IReadOnlyCollection<Move> Moves => _moves;
    public Move? LastMove => _moves.Count > 0 ? _moves[0] : null;
    public int Height { get; }
    public int Width { get; }

    public ChessBoard(
        Dictionary<Point, Piece> pieces,
        int height = GameConstants.BoardHeight,
        int width = GameConstants.BoardWidth
    )
    {
        Height = height;
        Width = width;

        _board = new Piece[height, width];
        InitializeBoard(pieces);
    }

    private void InitializeBoard(Dictionary<Point, Piece> pieces)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
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
            return GameLogicErrors.PieceNotFound;

        _board[from.Y, from.X] = null;
        _board[to.Y, to.X] = piece with { TimesMoved = piece.TimesMoved + 1 };
        return Result.Success;
    }

    public void ClearSquare(Point point) => _board[point.Y, point.X] = null;

    public bool IsWithinBoundaries(Point point) =>
        point.Y >= 0 && point.Y < Height && point.X >= 0 && point.X < Width;

    public IEnumerable<(Point Position, Piece? Piece)> EnumerateSquares()
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
