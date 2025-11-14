using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public class PredicateMoveCondition(Func<Move, bool> predicate) : IMoveCondition
{
    private readonly Func<Move, bool> _predicate = predicate;

    public bool Evaluate(Move move) => _predicate(move);
}
