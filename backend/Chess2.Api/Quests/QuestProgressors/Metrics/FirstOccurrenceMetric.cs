using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Metrics.FirstOccurrenceMetric")]
public class FirstOccurrenceMetric(Func<Move, GameQuestSnapshot, bool> predicate) : IQuestMetric
{
    [Id(0)]
    private readonly Func<Move, GameQuestSnapshot, bool> _predicate = predicate;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot)
    {
        for (int i = 0; i < snapshot.MoveHistory.Count; i++)
        {
            var move = snapshot.MoveHistory[i];
            if (_predicate(move, snapshot))
                return i;
        }
        return snapshot.MoveHistory.Count;
    }
}
