using System.Text;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.Services;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public class RegularNotator(IPieceToLetter pieceToLetter) : BaseSanNotator(pieceToLetter)
{
    public override SpecialMoveType HandlesMoveType => SpecialMoveType.None;

    public override void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        var isPawn = GameLogicConstants.PawnLikePieces.Contains(move.Piece.Type);
        var isCapture = move.Captures.Count != 0;

        sb.Append(PieceChar(move.Piece.Type));

        // if this is a pawn move AND a capture, add the file
        if (isPawn && isCapture)
            sb.Append(FileLetter(move.From.X));

        DisambiguatePosition(move, legalMoves, sb);
        NotateIntermediateSquares(move, sb);
        NotateDestination(move, sb);
        NotateSideCaptures(move, sb);
    }
}
