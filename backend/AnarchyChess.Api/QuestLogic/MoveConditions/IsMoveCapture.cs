using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.MoveConditions.IsMoveCapture")]
public class IsMoveCapture(int ofAtLeast = 1) : IMoveCondition
{
    [Id(0)]
    private readonly int _ofAtLeast = ofAtLeast;

    public bool Evaluate(Move move) => move.Captures.Count >= _ofAtLeast;
}
