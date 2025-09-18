using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;

namespace Chess2.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestConditions.OwnMoveOccurredCondition")]
public class OwnMoveOccurredCondition(params IMoveCondition[] moveConditions) : IQuestCondition
{
    [Id(0)]
    private readonly IMoveCondition[] _moveConditions = moveConditions;

    public bool Evaluate(GameQuestSnapshot snapshot)
    {
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.MoveHistory.Count; i += 2)
        {
            if (_moveConditions.All(x => x.Evaluate(snapshot.MoveHistory[i])))
                return true;
        }
        return false;
    }
}
