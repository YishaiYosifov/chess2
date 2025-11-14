using System.Text;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.Services;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

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
