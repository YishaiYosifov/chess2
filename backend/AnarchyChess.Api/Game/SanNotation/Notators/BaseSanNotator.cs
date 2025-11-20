using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public abstract class BaseSanNotator(IPieceToLetter pieceToLetter) : ISanNotator
{
    private readonly IPieceToLetter _pieceToLetter = pieceToLetter;

    public abstract SpecialMoveType HandlesMoveType { get; }

    public abstract void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb);

    protected static IEnumerable<Move> FindMovesAtSameDestination(
        Move move,
        IEnumerable<Move> legalMoves
    ) =>
        // moves where the same piece type moved to the same destination
        legalMoves.Where(x =>
            x.To == move.To && x.Piece.Type == move.Piece.Type && x.From != move.From
        );

    protected static char FileLetter(int x) => (char)('a' + x);

    protected string PieceChar(PieceType piece) =>
        GameLogicConstants.PawnLikePieces.Contains(piece)
            ? ""
            : _pieceToLetter.GetLetter(piece).ToUpper();

    protected static void DisambiguatePosition(
        Move move,
        IEnumerable<Move> legalMoves,
        StringBuilder sb
    )
    {
        var movesWithSameDestination = FindMovesAtSameDestination(move, legalMoves);

        var isRankAmbiguous = movesWithSameDestination.Any(x => x.From.Y == move.From.Y);
        var isFileAmbiguous = movesWithSameDestination.Any(x => x.From.X == move.From.X);

        if (isRankAmbiguous)
            sb.Append((char)('a' + move.From.X));
        if (isFileAmbiguous)
            sb.Append(move.From.Y + 1);
    }

    protected static void NotateDestination(Move move, StringBuilder sb)
    {
        var isCapture = move.Captures.Any(x => x.Position == move.To);
        if (isCapture)
            sb.Append('x');
        sb.Append(move.To.AsAlgebraic());
    }

    protected static void NotateIntermediateSquares(Move move, StringBuilder sb)
    {
        foreach (var square in move.IntermediateSquares)
        {
            sb.Append('~');
            sb.Append(square.Position.AsAlgebraic());
        }
    }

    protected static void NotateSideCaptures(Move move, StringBuilder sb)
    {
        // captures that are not the destination
        var sideCaptures = move.Captures.Where(x => x.Position != move.To);
        foreach (var capture in sideCaptures)
        {
            sb.Append('x');
            sb.Append(capture.Position.AsAlgebraic());
        }
    }
}
