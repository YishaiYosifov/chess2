using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.QuestLogic.MoveConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.MoveConditions.IsMoveOfType")]
public class IsMoveOfType(params HashSet<SpecialMoveType> moveTypes) : IMoveCondition
{
    [Id(0)]
    private readonly HashSet<SpecialMoveType> _moveTypes = moveTypes;

    public bool Evaluate(Move move) => _moveTypes.Contains(move.SpecialMoveType);
}
