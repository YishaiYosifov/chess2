using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame;
using System.Diagnostics.CodeAnalysis;

namespace Chess2.Api.GameLogic;

public class ChessBoard
{
    private readonly Piece?[,] _board;

    private readonly List<Move> _moves = [];

    public IReadOnlyList<Move> Moves => _moves;
    public int Height { get; }
    public int Width { get; }

    public ChessBoard(
        Dictionary<AlgebraicPoint, Piece>? pieces = null,
        int height = GameConstants.BoardHeight,
        int width = GameConstants.BoardWidth
    )
    {
        Height = height;
        Width = width;

        _board = new Piece[height, width];
        if (pieces is not null)
            InitializeBoard(pieces);
    }

    private void InitializeBoard(Dictionary<AlgebraicPoint, Piece> pieces)
    {
        foreach (var (pt, piece) in pieces)
        {
            if (IsWithinBoundaries(pt))
                _board[pt.Y, pt.X] = piece;
        }
    }

    public bool TryGetPieceAt(AlgebraicPoint point, [NotNullWhen(true)] out Piece? piece)
    {
        piece = null;
        if (!IsWithinBoundaries(point))
            return false;

        piece = _board[point.Y, point.X];
        return piece is not null;
    }

    public Piece? PeekPieceAt(AlgebraicPoint point) =>
        IsWithinBoundaries(point) ? _board[point.Y, point.X] : null;

    public bool IsEmpty(AlgebraicPoint point) =>
        !IsWithinBoundaries(point) || _board[point.Y, point.X] is null;

    public void PlayMove(Move move)
    {
        var steps = move.Flatten().ToList();
        foreach (var (from, to) in steps)
        {
            if (!IsWithinBoundaries(from) || !IsWithinBoundaries(to))
                throw new ArgumentOutOfRangeException(
                    nameof(move),
                    "Move is out of board boundaries"
                );

            if (!TryGetPieceAt(from, out var piece))
                throw new ArgumentException(
                    $"Piece not found at the specified 'From' point",
                    nameof(move)
                );
        }

        // apply captures first
        foreach (var capture in move.CapturedSquares)
        {
            _board[capture.Y, capture.X] = null;
        }

        // then move the pieces
        foreach (var (from, to) in steps)
        {
            // we can safely assume that the piece exists here, as we checked it before
            var piece = _board[from.Y, from.X]!;
            _board[to.Y, to.X] = piece with { TimesMoved = piece.TimesMoved + 1 };
            _board[from.Y, from.X] = null;
        }

        if (move.PromotesTo is PieceType promotesTo)
        {
            var promotionPiece = _board[move.To.Y, move.To.X]!;
            _board[move.To.Y, move.To.X] = promotionPiece with { Type = promotesTo };
        }

        _moves.Add(move);
    }

    public void PlacePiece(AlgebraicPoint point, Piece piece) => _board[point.Y, point.X] = piece;

    public bool IsWithinBoundaries(AlgebraicPoint point) =>
        point.Y >= 0 && point.Y < Height && point.X >= 0 && point.X < Width;

    public IEnumerable<(AlgebraicPoint Position, Piece? Occupant)> EnumerateSquares()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                yield return (new AlgebraicPoint(x, y), _board[y, x]);
            }
        }
    }
}
