using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

public class OccurrenceCountMetric(Func<Move, GameQuestSnapshot, bool> predicate) : IQuestProgressor
{
    private readonly Func<Move, GameQuestSnapshot, bool> _predicate = predicate;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot)
    {
        int count = 0;
        foreach (var move in snapshot.MoveHistory)
        {
            if (_predicate(move, snapshot))
                count++;
        }
        return count;
    }
}
