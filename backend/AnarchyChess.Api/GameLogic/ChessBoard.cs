using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.ChessBoard")]
public class ChessBoard : IReadOnlyChessBoard
{
    [Id(0)]
    private readonly Piece?[,] _board;

    [Id(1)]
    private readonly List<Move> _moves = [];

    [Id(2)]
    private readonly Dictionary<(PieceType, GameColor?), HashSet<AlgebraicPoint>> _piecePositions =
    [];

    public IReadOnlyList<Move> Moves => _moves;
    public GameColor SideToMove => _moves.Count % 2 == 0 ? GameColor.White : GameColor.Black;

    [Id(3)]
    public int Height { get; }

    [Id(4)]
    public int Width { get; }

    public ChessBoard(
        Dictionary<AlgebraicPoint, Piece>? pieces = null,
        int height = GameLogicConstants.BoardHeight,
        int width = GameLogicConstants.BoardWidth
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
            {
                PlacePiece(pt, piece);
            }
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

    public List<Piece> GetAllPiecesWith(PieceType type, GameColor? color)
    {
        (PieceType, GameColor?) key = (type, color);
        if (!_piecePositions.TryGetValue(key, out var positions))
            return [];

        List<Piece> result = [];
        foreach (var position in positions)
        {
            if (TryGetPieceAt(position, out var piece))
                result.Add(piece);
        }
        return result;
    }

    public void PlayMove(Move move)
    {
        var steps = move.Flatten().ToList();

        // store each piece along with its final destination before changing the board
        // ensures we don't lose any piece if its original square gets overwritten during moves
        List<(Piece piece, AlgebraicPoint newPosition)> finalPositions = [];
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

            finalPositions.Add((piece, to));
        }

        // step 1: remove all captured pieces first
        // doing this first prevents accidental self captures
        foreach (var capture in move.Captures)
        {
            RemovePiece(capture.Position);
        }

        // step 2: clear all origin squares of moving pieces
        // this is done before placing pieces to handle swaps correctly
        // prevents a piece from deleting another that just moved into its destination
        foreach (var (from, _) in steps)
        {
            RemovePiece(from);
        }

        // step 3: place all pieces in their final destinations
        foreach (var (piece, newPosition) in finalPositions)
        {
            PlacePiece(newPosition, piece with { TimesMoved = piece.TimesMoved + 1 });
        }

        foreach (var spawn in move.PieceSpawns)
        {
            PlacePiece(spawn.Position, new Piece(Type: spawn.Type, Color: spawn.Color));
        }

        if (move.PromotesTo is PieceType promotesTo)
        {
            ModifyPiece(move.To, piece => piece with { Type = promotesTo, TimesMoved = 0 });
        }

        _moves.Add(move);
    }

    public void PlacePiece(AlgebraicPoint point, Piece piece)
    {
        _board[point.Y, point.X] = piece;

        (PieceType, GameColor?) key = (piece.Type, piece.Color);
        if (_piecePositions.TryGetValue(key, out var positions))
            positions.Add(point);
        else
            _piecePositions[key] = [point];
    }

    public void RemovePiece(AlgebraicPoint point)
    {
        if (!TryGetPieceAt(point, out var piece))
            return;

        _board[point.Y, point.X] = null;

        (PieceType, GameColor?) key = (piece.Type, piece.Color);
        if (_piecePositions.TryGetValue(key, out var positions))
            positions.Remove(point);
    }

    public void ModifyPiece(AlgebraicPoint point, Func<Piece, Piece> modifyAction)
    {
        if (!TryGetPieceAt(point, out var piece))
            return;

        RemovePiece(point);
        PlacePiece(point, modifyAction(piece));
    }

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

    public IEnumerable<(AlgebraicPoint Position, Piece Occupant)> EnumeratePieces()
    {
        foreach (var positions in _piecePositions.Values)
        {
            foreach (var position in positions)
            {
                if (TryGetPieceAt(position, out var piece))
                    yield return (position, piece);
            }
        }
    }
}
