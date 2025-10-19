using Chess2.Api.GameLogic.Models;
using System.Diagnostics.CodeAnalysis;

namespace Chess2.Api.GameLogic;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.ChessBoard")]
public class ChessBoard
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

    public ChessBoard(ChessBoard board)
    {
        Height = board.Height;
        Width = board.Width;
        _board = (Piece?[,])board._board.Clone();
        _moves = [.. board._moves];

        foreach (var kvp in board._piecePositions)
        {
            _piecePositions[kvp.Key] = [.. kvp.Value];
        }
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
            RemovePiece(capture.Position);
        }

        // then move the pieces
        foreach (var (from, to) in steps)
        {
            MovePiece(from, to);
        }

        foreach (var spawn in move.PieceSpawns)
        {
            PlacePiece(spawn.Position, new Piece(Type: spawn.Type, Color: spawn.Color));
        }

        if (move.PromotesTo is PieceType promotesTo)
        {
            ModifyPiece(move.To, piece => piece with { Type = promotesTo });
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

    public void MovePiece(AlgebraicPoint from, AlgebraicPoint to)
    {
        if (!TryGetPieceAt(from, out var piece))
            return;

        RemovePiece(from);
        PlacePiece(to, piece with { TimesMoved = piece.TimesMoved + 1 });
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
