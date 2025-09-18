using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.MoveConditions;

namespace Chess2.Api.TestInfrastructure.Utils;

public class PredicateMoveCondition(Func<Move, bool> predicate) : IMoveCondition
{
    private readonly Func<Move, bool> _predicate = predicate;

    public bool Evaluate(Move move) => _predicate(move);
}
