using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMoveOfPiece")]
public class IsMoveOfPiece(PieceType piece) : IMoveCondition
{
    [Id(0)]
    private readonly PieceType _piece = piece;

    public bool Evaluate(Move move) => move.Piece.Type == _piece;
}
