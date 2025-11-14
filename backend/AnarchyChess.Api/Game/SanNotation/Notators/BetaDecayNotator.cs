using System.Text;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.Services;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

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
