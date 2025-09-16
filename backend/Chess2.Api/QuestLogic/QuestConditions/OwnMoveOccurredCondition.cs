using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestConditions.OwnMoveOccurredCondition")]
public class OwnMoveOccurredCondition(Func<Move, GameQuestSnapshot, bool> predicate)
    : IQuestCondition
{
    [Id(0)]
    private readonly Func<Move, GameQuestSnapshot, bool> _predicate = predicate;

    public bool Evaluate(GameQuestSnapshot snapshot)
    {
        int startIdx = snapshot.PlayerColor.Match(whenWhite: 0, whenBlack: 1);
        for (int i = startIdx; i < snapshot.MoveHistory.Count; i += 2)
        {
            if (_predicate(snapshot.MoveHistory[i], snapshot))
                return true;
        }
        return false;
    }
}
