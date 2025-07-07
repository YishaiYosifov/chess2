using System.Text;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface ISanCalculator
{
    string CalculateSan(Move move, IEnumerable<Move> legalMoves);
}

public class SanCalculator(IPieceToLetter pieceToLetter) : ISanCalculator
{
    private readonly IPieceToLetter _pieceToLetter = pieceToLetter;

    public string CalculateSan(Move move, IEnumerable<Move> legalMoves)
    {
        StringBuilder sb = new();

        var isPawn = move.Piece.Type == PieceType.Pawn;
        var isCapture = move.CapturedSquares?.Any() == true;

        // add the piece letter if this is not a pawn move
        if (!isPawn)
            sb.Append(_pieceToLetter.GetLetter(move.Piece.Type).ToUpper());

        // if this is a pawn move AND a capture, add the rank
        if (isPawn && isCapture)
            sb.Append(FileLetter(move.From.X));

        sb.Append(DisambiguatePosition(move, legalMoves));
        sb.Append(NotateDestination(move));
        sb.Append(NotateSideCaptures(move));

        return sb.ToString();
    }

    private static string DisambiguatePosition(Move move, IEnumerable<Move> legalMoves)
    {
        StringBuilder sb = new();
        var movesWithSameDestination = FindMovesAtSameDestination(move, legalMoves);

        var isRankAmbiguous = movesWithSameDestination.Any(x => x.From.Y == move.From.Y);
        var isFileAmbiguous = movesWithSameDestination.Any(x => x.From.X == move.From.X);

        if (isRankAmbiguous)
            sb.Append((char)('a' + move.From.X));
        if (isFileAmbiguous)
            sb.Append(move.From.Y + 1);

        return sb.ToString();
    }

    private static string NotateDestination(Move move)
    {
        StringBuilder sb = new();

        var isCapture = move.CapturedSquares?.Any(x => x == move.To) == true;
        if (isCapture)
            sb.Append('x');
        sb.Append(move.To.AsAlgebraic());

        return sb.ToString();
    }

    private static string NotateSideCaptures(Move move)
    {
        if (move.CapturedSquares is null)
            return "";

        StringBuilder sb = new();

        // captures that are not the destination
        var sideCaptures = move.CapturedSquares.Where(x => x != move.To);
        foreach (var capture in sideCaptures)
        {
            sb.Append('x');
            sb.Append(capture.AsAlgebraic());
        }

        return sb.ToString();
    }

    private static IEnumerable<Move> FindMovesAtSameDestination(
        Move move,
        IEnumerable<Move> legalMoves
    ) =>
        // moves where the same piece type moved to the same destination
        legalMoves.Where(x =>
            x.To == move.To && x.Piece.Type == move.Piece.Type && x.From != move.From
        );

    private static char FileLetter(int x) => (char)('a' + x);
}
