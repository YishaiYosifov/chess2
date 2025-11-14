using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic.ForeverRules;

public interface IForeveRule
{
    IEnumerable<Move> GetBehaviours(IReadOnlyChessBoard board, GameColor movingPlayer);
}
