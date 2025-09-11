using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors.Metrics;

public class OccurrenceCountMetric(Func<MoveSnapshot, GameState, GameColor, bool> predicate)
    : IQuestProgressor
{
    private readonly Func<MoveSnapshot, GameState, GameColor, bool> _predicate = predicate;

    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor)
    {
        int count = 0;
        foreach (var move in snapshot.MoveHistory)
        {
            if (_predicate(move, snapshot, playerColor))
                count++;
        }
        return count;
    }
}
