using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;

namespace AnarchyChess.Api.QuestLogic.QuestMetrics;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestMetrics.OwnMoveCountMetric")]
public class OwnMoveCountMetric(params IMoveCondition[] moveConditions) : IQuestMetric
{
    [Id(0)]
    private readonly IMoveCondition[] _moveConditions = moveConditions;

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        int count = 0;
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.MoveHistory.Count; i += 2)
        {
            if (_moveConditions.All(x => x.Evaluate(snapshot.MoveHistory[i])))
                count++;
        }
        return count;
    }
}
