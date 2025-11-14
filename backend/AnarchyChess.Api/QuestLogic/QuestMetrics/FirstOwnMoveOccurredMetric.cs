using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.FirstOwnMoveOccurredMetric")]
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
