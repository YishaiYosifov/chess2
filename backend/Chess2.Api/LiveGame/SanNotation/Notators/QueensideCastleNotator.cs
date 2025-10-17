using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Services;

namespace Chess2.Api.LiveGame.SanNotation.Notators;

public class QueensideCastleNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.QueensideCastle;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        sb.Append("O-O-O");
        foreach (var capture in move.Captures)
        {
            sb.Append('x');
            sb.Append(capture.Position.AsAlgebraic());
        }
    }
}
