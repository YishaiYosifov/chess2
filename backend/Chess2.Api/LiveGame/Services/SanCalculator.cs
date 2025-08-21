using System.Text;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.LiveGame.Services;

public interface ISanCalculator
{
    string CalculateSan(Move move, IEnumerable<Move> legalMoves, bool isKingCapture = false);
}

public class SanCalculator(IPieceToLetter pieceToLetter) : ISanCalculator
{
    private readonly IPieceToLetter _pieceToLetter = pieceToLetter;

    public string CalculateSan(Move move, IEnumerable<Move> legalMoves, bool isKingCapture = false)
    {
        StringBuilder sb = new();
        switch (move.SpecialMoveType)
        {
            case SpecialMoveType.KingsideCastle:
                NotateCastle(move, isKingside: true, sb);
                break;
            case SpecialMoveType.QueensideCastle:
                NotateCastle(move, isKingside: false, sb);
                break;
            default:
                NotateRegularMove(move, legalMoves, sb);
                break;
        }

        if (move.PromotesTo is PieceType promotesTo)
        {
            sb.Append('=');
            sb.Append(_pieceToLetter.GetLetter(promotesTo).ToUpper());
        }

        if (isKingCapture)
            sb.Append('#');

        return sb.ToString();
    }

    private void NotateRegularMove(Move move, IEnumerable<Move> legalMoves, StringBuilder sb)
    {
        var isPawn = move.Piece.Type == PieceType.Pawn || move.Piece.Type == PieceType.UnderagePawn;
        var isCapture = move.CapturedSquares.Any();

        // add the piece letter if this is not a pawn move
        if (!isPawn)
            sb.Append(_pieceToLetter.GetLetter(move.Piece.Type).ToUpper());

        // if this is a pawn move AND a capture, add the file
        if (isPawn && isCapture)
            sb.Append(FileLetter(move.From.X));

        DisambiguatePosition(move, legalMoves, sb);
        NotateDestination(move, sb);
        NotateSideCaptures(move, sb);
    }

    private static void NotateCastle(Move move, bool isKingside, StringBuilder sb)
    {
        sb.Append(isKingside ? "O-O" : "O-O-O");
        foreach (var capture in move.CapturedSquares)
        {
            sb.Append('x');
            sb.Append(capture.AsAlgebraic());
        }
    }

    private static void DisambiguatePosition(
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

    private static void NotateDestination(Move move, StringBuilder sb)
    {
        var isCapture = move.CapturedSquares.Any(x => x == move.To);
        if (isCapture)
            sb.Append('x');
        sb.Append(move.To.AsAlgebraic());
    }

    private static void NotateSideCaptures(Move move, StringBuilder sb)
    {
        // captures that are not the destination
        var sideCaptures = move.CapturedSquares.Where(x => x != move.To);
        foreach (var capture in sideCaptures)
        {
            sb.Append('x');
            sb.Append(capture.AsAlgebraic());
        }
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
