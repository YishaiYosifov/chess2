using Chess2.Api.Game;
using Chess2.Api.GameLogic.Errors;
using Chess2.Api.GameLogic.Models;
using ErrorOr;
using System.Diagnostics.CodeAnalysis;

namespace Chess2.Api.GameLogic;

public class ChessBoard
{
    private readonly Piece?[,] _board;

    private readonly List<Move> _moves = [];

    public IReadOnlyCollection<Move> Moves => _moves;
    public Move? LastMove => _moves.Count > 0 ? _moves[^1] : null;
    public int Height { get; }
    public int Width { get; }

    public ChessBoard(
        Dictionary<Point, Piece>? pieces = null,
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

    private void InitializeBoard(Dictionary<Point, Piece> pieces)
    {
        foreach (var (pt, piece) in pieces)
        {
            if (IsWithinBoundaries(pt))
                _board[pt.Y, pt.X] = piece;
        }
    }

    public bool TryGetPieceAt(Point point, [NotNullWhen(true)] out Piece? piece)
    {
        piece = _board[point.Y, point.X];
        return piece is not null;
    }

    public Piece? PeekPieceAt(Point point) =>
        IsWithinBoundaries(point) ? _board[point.Y, point.X] : null;

    public bool IsEmpty(Point point) =>
        !IsWithinBoundaries(point) || _board[point.Y, point.X] is null;

    public ErrorOr<Success> PlayMove(Move move)
    {
        var tempBoard = (Piece?[,])_board.Clone();
        var playResult = ExecuteMoveRecursive(move, tempBoard);
        if (playResult.IsError)
            return playResult.Errors;

        // apply the move to the actual board
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _board[y, x] = tempBoard[y, x];
        _moves.Add(move);

        return Result.Success;
    }

    private ErrorOr<Success> ExecuteMoveRecursive(Move move, Piece?[,] board)
    {
        if (!IsWithinBoundaries(move.To) || !IsWithinBoundaries(move.From))
            return GameLogicErrors.PointOutOfBound;

        var piece = board[move.From.Y, move.From.X];
        if (piece is null)
            return GameLogicErrors.PieceNotFound;

        foreach (var sideEffect in move.SideEffects ?? [])
        {
            var playResult = ExecuteMoveRecursive(sideEffect, board);
            if (playResult.IsError)
                return playResult.Errors;
        }

        foreach (var capture in move.CapturedSquares ?? [])
        {
            board[capture.Y, capture.X] = null;
        }
        board[move.To.Y, move.To.X] = piece with { TimesMoved = piece.TimesMoved + 1 };
        board[move.From.Y, move.From.X] = null;

        return Result.Success;
    }

    public void PlacePiece(Point point, Piece piece) => _board[point.Y, point.X] = piece;

    public bool IsWithinBoundaries(Point point) =>
        point.Y >= 0 && point.Y < Height && point.X >= 0 && point.X < Width;

    public IEnumerable<(Point Position, Piece? Piece)> EnumerateSquares()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                yield return (new Point(x, y), _board[y, x]);
            }
        }
    }
}
