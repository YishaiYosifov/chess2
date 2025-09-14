using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestMetrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestMetrics.GameLengthMetric")]
public class GameLengthMetric : IQuestMetric
{
    public int Evaluate(GameQuestSnapshot snapshot) => snapshot.MoveHistory.Count;
}
