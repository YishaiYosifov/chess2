using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.MoveConditions.IsMoveCapture")]
public class IsMoveCapture(int ofAtLeast = 1) : IMoveCondition
{
    [Id(0)]
    private readonly int _ofAtLeast = ofAtLeast;

    public bool Evaluate(Move move) => move.Captures.Count >= _ofAtLeast;
}
