using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestMetrics;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestMetrics.OccurrenceCountMetric")]
public class OccurrenceCountMetric(Func<Move, GameQuestSnapshot, bool> predicate) : IQuestMetric
{
    [Id(0)]
    private readonly Func<Move, GameQuestSnapshot, bool> _predicate = predicate;

    public int Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.MoveHistory.Count(m => _predicate(m, snapshot));
}
