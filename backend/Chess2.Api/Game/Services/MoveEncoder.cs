using System.Text;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IMoveEncoder
{
    IEnumerable<string> EncodeMoves(IEnumerable<Move> moves);
    string EncodeSingleMove(Move move);
}

public class MoveEncoder : IMoveEncoder
{
    /// <summary>
    /// Encodes a list of legal moves into a compact legal moves string,
    /// where each move include:
    /// - The starting square (<see cref="Move.From"/>)
    /// - Any intermediate squares the piece moves through (<see cref="Move.Through"/>),
    /// - The destination square (<see cref="Move.To"/>)
    /// - Any side effect moves (<see cref="Move.SideEffects"/>), appended after a '-' separator,
    ///   recursively encoded with the same format
    /// - Any captured squares (<see cref="Move.CapturedSquares"/>), appended after "!"
    ///
    /// Example encoding for castling with rook side effect and a capture:
    /// <code>e1f1g1-h1f1!d4</code>
    /// </summary>
    public IEnumerable<string> EncodeMoves(IEnumerable<Move> moves) =>
        moves.Select(EncodeSingleMove);

    public string EncodeSingleMove(Move move)
    {
        var path = new StringBuilder();
        BuildPath(move, path);

        return path.ToString();
    }

    private static void BuildPath(Move move, StringBuilder path)
    {
        path.Append(move.From.AsAlgebraic());
        foreach (var throughPoint in move.Through ?? [])
            path.Append(throughPoint.AsAlgebraic());
        path.Append(move.To.AsAlgebraic());

        foreach (var capture in move.CapturedSquares ?? [])
        {
            path.Append('!');
            path.Append(capture.AsAlgebraic());
        }

        foreach (var sideEffect in move.SideEffects ?? [])
        {
            path.Append('-');
            BuildPath(sideEffect, path);
        }
    }
}
