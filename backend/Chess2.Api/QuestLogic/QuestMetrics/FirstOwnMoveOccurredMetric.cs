using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;

namespace Chess2.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestMetrics.FirstOwnMoveOccurredMetric")]
public class FirstOwnMoveOccurredMetric(params IMoveCondition[] moveConditions) : IQuestMetric
{
    [Id(0)]
    private readonly IMoveCondition[] _moveCondition = moveConditions;

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.MoveHistory.Count; i += 2)
        {
            var move = snapshot.MoveHistory[i];
            if (_moveCondition.All(x => x.Evaluate(move)))
                return i;
        }
        return int.MaxValue;
    }
}
