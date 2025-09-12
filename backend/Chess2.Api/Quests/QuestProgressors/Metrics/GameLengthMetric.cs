using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Metrics.GameLengthMetric")]
public class GameLengthMetric : IQuestProgressor
{
    public int EvaluateProgressMade(GameQuestSnapshot snapshot) => snapshot.MoveHistory.Count;
}
