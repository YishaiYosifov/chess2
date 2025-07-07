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

        if (move.Piece.Type != PieceType.Pawn)
            sb.Append(_pieceToLetter.GetLetter(move.Piece.Type).ToUpper());

        sb.Append(DisambiguatePosition(move, legalMoves));
        sb.Append(move.To.AsAlgebraic());

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

    private static IEnumerable<Move> FindMovesAtSameDestination(
        Move move,
        IEnumerable<Move> legalMoves
    ) =>
        // moves where the same piece type moved to the same destination
        legalMoves.Where(x =>
            x.To == move.To && x.Piece.Type == move.Piece.Type && x.From != move.From
        );
}
