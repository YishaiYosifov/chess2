using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.Services;

namespace Chess2.Api.Game.SanNotation.Notators;

public class VerticalCastleNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.VerticalCastle;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append("O-O-O-O-O-O");
        foreach (var capture in move.Captures)
        {
            sb.Append('x');
            sb.Append(capture.Position.AsAlgebraic());
        }
    }
}
