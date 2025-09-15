using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestMetrics;

public interface IQuestMetric
{
    int Evaluate(GameQuestSnapshot snapshot);
}
