using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame;

namespace Chess2.Api.GameLogic;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.ChessBoard")]
public class ChessBoard
{
    [Id(0)]
    private readonly Piece?[,] _board;

    [Id(1)]
    private readonly List<Move> _moves = [];

    public IReadOnlyList<Move> Moves => _moves;
    public GameColor SideToMove => _moves.Count % 2 == 0 ? GameColor.White : GameColor.Black;

    [Id(3)]
    public int Height { get; private set; }

    [Id(4)]
    public int Width { get; private set; }

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

    public ChessBoard(ChessBoard board)
    {
        Height = board.Height;
        Width = board.Width;
        _board = (Piece?[,])board._board.Clone();
        _moves = [.. board._moves];
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
        foreach (var capture in move.Captures)
        {
            _board[capture.Position.Y, capture.Position.X] = null;
        }

        // then move the pieces
        foreach (var (from, to) in steps)
        {
            if (!TryGetPieceAt(from, out var piece))
                continue;

            _board[to.Y, to.X] = piece with { TimesMoved = piece.TimesMoved + 1 };
            _board[from.Y, from.X] = null;
        }

        foreach (var spawn in move.PieceSpawns)
        {
            if (IsWithinBoundaries(spawn.Position))
            {
                _board[spawn.Position.Y, spawn.Position.X] = new Piece(
                    Type: spawn.Type,
                    Color: spawn.Color
                );
            }
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
