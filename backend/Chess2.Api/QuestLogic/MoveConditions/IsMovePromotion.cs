using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.MoveConditions.IsMovePromotion")]
public class IsMovePromotion : IMoveCondition
{
    public bool Evaluate(Move move) => move.PromotesTo is not null;
}
