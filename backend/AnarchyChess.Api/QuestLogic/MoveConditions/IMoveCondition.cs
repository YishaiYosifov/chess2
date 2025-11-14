using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

public interface IMoveCondition
{
    /// <summary>
    /// Determines whether a move satisfies a specific condition.
    /// </summary>
    /// <returns>True if the move meets the condition, otherwise false.</returns>
    bool Evaluate(Move move);
}
