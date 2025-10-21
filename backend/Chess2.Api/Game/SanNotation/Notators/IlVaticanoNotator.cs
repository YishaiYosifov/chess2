using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.Services;

namespace Chess2.Api.Game.SanNotation.Notators;

public class IlVaticanoNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.IlVaticano;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        sb.Append("B-O-O-B");
}
