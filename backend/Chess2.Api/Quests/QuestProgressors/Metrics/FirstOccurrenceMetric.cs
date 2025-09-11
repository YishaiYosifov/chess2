using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Metrics.FirstOccurrenceMetric")]
public class FirstOccurrenceMetric(Func<MoveSnapshot, GameState, GameColor, bool> predicate)
    : IQuestProgressor
{
    [Id(0)]
    private readonly Func<MoveSnapshot, GameState, GameColor, bool> _predicate = predicate;

    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor)
    {
        for (int i = 0; i < snapshot.MoveHistory.Count; i++)
        {
            var move = snapshot.MoveHistory[i];
            if (_predicate(move, snapshot, playerColor))
                return i;
        }
        return snapshot.MoveHistory.Count;
    }
}
