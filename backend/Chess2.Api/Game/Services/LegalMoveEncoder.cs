using System.Text;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface ILegalMoveEncoder
{
    string EncodeLegalMoves(IEnumerable<Move> legalMoves);
}

public class LegalMoveEncoder : ILegalMoveEncoder
{
    /// <summary>
    /// Encodes a list of legal moves into a compact legal moves string,
    /// where each move include:
    /// - The starting square (<see cref="Move.From"/>)
    /// - Any intermediate squares the piece moves through (<see cref="Move.Through"/>),
    ///   separated by '>' characters to indicate path traversal
    /// - The destination square (<see cref="Move.To"/>)
    /// - Any side effect moves (<see cref="Move.SideEffects"/>), appended after a '-' separator,
    ///   recursively encoded with the same format
    /// - Any explicitly captured squares (<see cref="Move.CapturedSquares"/>), appended after "!"
    ///
    /// Example encoding for castling with rook side effect and a capture:
    /// <code>e1>f1g1!d5-h1f1</code>
    /// </summary>
    public string EncodeLegalMoves(IEnumerable<Move> legalMoves)
    {
        StringBuilder sb = new();
        foreach (var move in legalMoves)
        {
            var encodedMove = EncodeSingleMove(move);
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(encodedMove);
        }

        return sb.ToString();
    }

    private static string EncodeSingleMove(Move move)
    {
        StringBuilder sb = new();
        sb.Append(move.From.AsUCI());
        foreach (var throughPoint in move.Through ?? [])
        {
            sb.Append('>');
            sb.Append(throughPoint.AsUCI());
        }
        sb.Append(move.To.AsUCI());

        foreach (var capture in move.CapturedSquares ?? [])
        {
            sb.Append('!');
            sb.Append(capture.AsUCI());
        }

        foreach (var sideEffect in move.SideEffects ?? [])
        {
            var encodedSideEffect = EncodeSingleMove(sideEffect);
            sb.Append('-');
            sb.Append(encodedSideEffect);
        }

        return sb.ToString();
    }
}
