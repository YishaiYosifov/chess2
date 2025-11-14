using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

public interface IQuestCondition
{
    /// <summary>
    /// Checks whether a specific condition is true for the given game state.
    /// </summary>
    /// <returns>True if the condition is met, otherwise false.</returns>
    bool Evaluate(GameQuestSnapshot snapshot);
}
