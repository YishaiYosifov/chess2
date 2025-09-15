using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestMetrics.GameLengthMetric")]
public class MoveCountMetric : IQuestMetric
{
    public int Evaluate(GameQuestSnapshot snapshot) => snapshot.MoveHistory.Count / 2;
}
