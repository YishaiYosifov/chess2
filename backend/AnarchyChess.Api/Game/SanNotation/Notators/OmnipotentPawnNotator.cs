using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.Services;
using System.Text;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class OmnipotentPawnNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.OmnipotentPawnSpawn;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        NotateDestination(move, sb);
}
