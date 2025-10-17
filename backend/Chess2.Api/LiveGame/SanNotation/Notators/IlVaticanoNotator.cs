using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Services;

namespace Chess2.Api.LiveGame.SanNotation.Notators;

public class IlVaticanoNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.IlVaticano;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        sb.Append("B-O-O-B");
}
