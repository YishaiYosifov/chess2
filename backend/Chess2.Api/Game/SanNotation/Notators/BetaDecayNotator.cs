using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.Services;

namespace Chess2.Api.Game.SanNotation.Notators;

public class BetaDecayNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.RadioactiveBetaDecay;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append(PieceChar(move.Piece.Type));
        sb.Append('β');

        foreach (var spawn in move.PieceSpawns)
        {
            sb.Append(PieceChar(spawn.Type));
            sb.Append(spawn.Position.AsAlgebraic());
        }
    }
}
