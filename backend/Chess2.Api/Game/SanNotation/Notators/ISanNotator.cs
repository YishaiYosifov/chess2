using System.Text;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.SanNotation.Notators;

public interface ISanNotator
{
    SpecialMoveType HandlesMoveType { get; }
    void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb);
}
