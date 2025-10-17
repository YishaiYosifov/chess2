using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.PieceMovementRules;

public class RadioactiveBetaDecayRule(params PieceType[] decayInto) : IPieceMovementRule
{
    private readonly PieceType[] _decayInto = decayInto;

    public IEnumerable<Move> Evaluate(ChessBoard board, AlgebraicPoint position, Piece movingPiece)
    {
        if (!IsRankOtherwiseEmpty(position, board))
            yield break;

        var rankCenter = board.Width / 2;
        Offset offset = position.X < rankCenter ? new(-1, 0) : new(1, 0);
        AlgebraicPoint spawnPosition =
            position.X < rankCenter ? new(board.Width - 1, position.Y) : new(0, position.Y);

        List<PieceSpawn> spawns = [];
        foreach (var pieceType in _decayInto)
        {
            if (!board.IsWithinBoundaries(spawnPosition))
                yield break;

            spawns.Add(
                new PieceSpawn(Type: pieceType, Color: movingPiece.Color, Position: spawnPosition)
            );
            spawnPosition += offset;
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

    private static bool IsRankOtherwiseEmpty(AlgebraicPoint position, ChessBoard board)
    {
        for (var file = 0; file < board.Width; file++)
        {
            AlgebraicPoint checkPosition = new(file, position.Y);
            if (checkPosition != position && !board.IsEmpty(checkPosition))
                return false;
        }
        return true;
    }
}
