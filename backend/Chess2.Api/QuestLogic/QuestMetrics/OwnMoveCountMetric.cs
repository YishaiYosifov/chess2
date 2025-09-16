using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestMetrics.OwnMoveCountMetric")]
public class OwnMoveCountMetric(Func<Move, GameQuestSnapshot, bool> predicate) : IQuestMetric
{
    [Id(0)]
    private readonly Func<Move, GameQuestSnapshot, bool> _predicate = predicate;

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        int count = 0;
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.MoveHistory.Count; i += 2)
        {
            if (_predicate(snapshot.MoveHistory[i], snapshot))
                count++;
        }
        return count;
    }
}
