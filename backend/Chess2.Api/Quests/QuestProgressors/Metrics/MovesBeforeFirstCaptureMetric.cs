using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Metrics.MovesBeforeFirstCaptureMetric")]
public class MovesBeforeFirstCaptureMetric : IQuestProgressor
{
    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor)
    {
        for (int i = 0; i < snapshot.MoveHistory.Count; i++)
        {
            var move = snapshot.MoveHistory[i];
            if (move.Path.CapturedIdxs?.Count > 0)
                return i;
        }
        return snapshot.MoveHistory.Count;
    }
}
