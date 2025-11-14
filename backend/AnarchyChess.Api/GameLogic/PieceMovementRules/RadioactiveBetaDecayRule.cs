using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.PieceMovementRules;

public class RadioactiveBetaDecayRule(Dictionary<Offset, PieceType> decays) : IPieceMovementRule
{
    private readonly Dictionary<Offset, PieceType> _decays = decays;

    public IEnumerable<Move> Evaluate(
        IReadOnlyChessBoard board,
        AlgebraicPoint position,
        Piece movingPiece
    )
    {
        List<PieceSpawn> spawns = [];
        foreach (var (offset, pieceType) in _decays)
        {
            var spawnPosition = position + offset;
            if (!board.IsWithinBoundaries(spawnPosition) || !board.IsEmpty(spawnPosition))
                yield break;

            spawns.Add(
                new PieceSpawn(Type: pieceType, Color: movingPiece.Color, Position: spawnPosition)
            );
        }

        yield return new Move(
            from: position,
            to: position,
            piece: movingPiece,
            captures: [new MoveCapture(position, board)],
            pieceSpawns: spawns,
            specialMoveType: SpecialMoveType.RadioactiveBetaDecay
        );
    }
}
