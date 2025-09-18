using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.QuestLogic.MoveConditions;

public interface IMoveCondition
{
    bool Evaluate(Move move);
}
