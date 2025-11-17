using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMoveOfPiece")]
public class IsMoveOfPiece(params HashSet<PieceType> pieces) : IMoveCondition
{
    [Id(0)]
    private readonly HashSet<PieceType> _pieces = pieces;

    public bool Evaluate(Move move) => _pieces.Contains(move.Piece.Type);
}
