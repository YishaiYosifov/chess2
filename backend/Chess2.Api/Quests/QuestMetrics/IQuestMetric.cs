using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestMetrics;

public interface IQuestMetric
{
    int Evaluate(GameQuestSnapshot snapshot);
}
