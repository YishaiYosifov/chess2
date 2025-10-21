using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic;

public interface IReadOnlyChessBoard
{
    int Width { get; }
    int Height { get; }
    IReadOnlyList<Move> Moves { get; }
    GameColor SideToMove { get; }

    bool TryGetPieceAt(AlgebraicPoint point, [NotNullWhen(true)] out Piece? piece);
    Piece? PeekPieceAt(AlgebraicPoint point);
    bool IsWithinBoundaries(AlgebraicPoint point);
    bool IsEmpty(AlgebraicPoint point);
    List<Piece> GetAllPiecesWith(PieceType type, GameColor? color);

    IEnumerable<(AlgebraicPoint Position, Piece? Occupant)> EnumerateSquares();
    IEnumerable<(AlgebraicPoint Position, Piece Occupant)> EnumeratePieces();
}
