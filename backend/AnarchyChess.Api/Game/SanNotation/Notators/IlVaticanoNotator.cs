using System.Text;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.Services;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class IlVaticanoNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.IlVaticano;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        sb.Append("B-O-O-B");
}
