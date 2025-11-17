using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

public class OpponentMoveOccurredCondition(params IMoveCondition[] moveConditions) : IQuestCondition
{
    private readonly IMoveCondition[] _moveConditions = moveConditions;

    public bool Evaluate(GameQuestSnapshot snapshot)
    {
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 1, whenBlack: 0);
        for (int i = startIdx; i < snapshot.MoveHistory.Count; i += 2)
        {
            if (_moveConditions.All(x => x.Evaluate(snapshot.MoveHistory[i])))
                return true;
        }
        return false;
    }
}
