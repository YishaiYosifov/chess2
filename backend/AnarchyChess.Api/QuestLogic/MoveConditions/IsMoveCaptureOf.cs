using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMoveCaptureOf")]
public class IsMoveCaptureOf(PieceType piece) : IMoveCondition
{
    [Id(0)]
    private readonly PieceType _piece = piece;

    public bool Evaluate(Move move) => move.Captures.Any(c => c.CapturedPiece.Type == _piece);
}
