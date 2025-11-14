using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMovePromotion")]
public class IsMovePromotion : IMoveCondition
{
    public bool Evaluate(Move move) => move.PromotesTo is not null;
}
