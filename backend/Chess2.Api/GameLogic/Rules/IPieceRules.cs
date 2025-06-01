using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic.Rules;

public interface IPieceRules
{
    public void IsValid(ChessBoard board, Point From, Point To);
}
