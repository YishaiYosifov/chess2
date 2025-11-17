using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMovePromotion")]
public class IsMovePromotion(PieceType? promotesTo = null) : IMoveCondition
{
    [Id(0)]
    private readonly PieceType? _promotesTo = promotesTo;

    public bool Evaluate(Move move) =>
        _promotesTo is null ? move.PromotesTo is not null : move.PromotesTo == _promotesTo;
}
