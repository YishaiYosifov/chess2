using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.ForeverRules;

public interface IForeveRule
{
    IEnumerable<Move> GetBehaviours(ChessBoard board, GameColor movingPlayer);
}
