using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Metrics.GameLengthMetric")]
public class GameLengthMetric : IQuestProgressor
{
    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor) =>
        snapshot.MoveHistory.Count;
}
