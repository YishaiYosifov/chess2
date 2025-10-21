using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.Services;
using System.Text;

namespace Chess2.Api.Game.SanNotation.Notators;

public class OmnipotentPawnNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.OmnipotentPawnSpawn;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb) =>
        NotateDestination(move, sb);
}
