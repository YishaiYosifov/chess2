using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

public class AndGate(IQuestProgressor left, IQuestProgressor right) : IQuestGate
{
    private readonly IQuestProgressor _left = left;
    private readonly IQuestProgressor _right = right;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot)
    {
        var leftResult = _left.EvaluateProgressMade(snapshot);
        if (leftResult < 1)
            return 0;

        var rightResult = _right.EvaluateProgressMade(snapshot);
        if (rightResult < 1)
            return 0;

        return 1;
    }
}
