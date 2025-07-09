using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Game;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic;

public class ChessBoard
{
    private readonly Piece?[,] _board;

    private readonly List<Move> _moves = [];

    public IEnumerable<Move> Moves => _moves;
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
        foreach (var step in steps)
        {
            if (!IsWithinBoundaries(step.From) || !IsWithinBoundaries(step.To))
                throw new ArgumentOutOfRangeException(
                    nameof(move),
                    "Move is out of board boundaries"
                );

            if (!TryGetPieceAt(step.From, out var piece) || piece.Type != step.Piece.Type)
                throw new ArgumentException(
                    $"Piece {step.Piece.Type} not found at the specified 'From' point",
                    nameof(move)
                );
        }

        // apply captures first
        foreach (var step in steps)
        {
            if (step.CapturedSquares is null)
                continue;

            foreach (var capture in step.CapturedSquares)
                _board[capture.Y, capture.X] = null;
        }

        // then move the pieces
        foreach (var step in steps)
        {
            // we can safely assume that the piece exists here, as we checked it before
            var piece = _board[step.From.Y, step.From.X]!;
            _board[step.To.Y, step.To.X] = piece with { TimesMoved = piece.TimesMoved + 1 };
            _board[step.From.Y, step.From.X] = null;
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
