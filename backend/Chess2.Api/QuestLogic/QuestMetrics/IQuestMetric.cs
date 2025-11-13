using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestMetrics;

public interface IQuestMetric
{
    /// <summary>
    /// Calculates a value based on the current game state.
    /// </summary>
    /// <returns>
    /// A number representing how often a specific event occurred,
    /// int.MaxValue if it didn't occur
    /// </returns>
    int Evaluate(GameQuestSnapshot snapshot);
}
