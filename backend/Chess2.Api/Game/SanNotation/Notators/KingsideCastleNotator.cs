using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.Services;

namespace Chess2.Api.Game.SanNotation.Notators;

public class KingsideCastleNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.KingsideCastle;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append("O-O");
        foreach (var capture in move.Captures)
        {
            sb.Append('x');
            sb.Append(capture.Position.AsAlgebraic());
        }
    }
}
