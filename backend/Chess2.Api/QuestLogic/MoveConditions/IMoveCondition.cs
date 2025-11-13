using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.QuestLogic.MoveConditions;

public interface IMoveCondition
{
    /// <summary>
    /// Determines whether a move satisfies a specific condition.
    /// </summary>
    /// <returns>True if the move meets the condition, otherwise false.</returns>
    bool Evaluate(Move move);
}
