using System.Text;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface ILegalMoveEncoder
{
    IEnumerable<string> EncodeLegalMoves(IEnumerable<Move> legalMoves);
}

public class LegalMoveEncoder : ILegalMoveEncoder
{
    /// <summary>
    /// Encodes a list of legal moves into a compact legal moves string,
    /// where each move include:
    /// - The starting square (<see cref="Move.From"/>)
    /// - Any intermediate squares the piece moves through (<see cref="Move.Through"/>),
    /// - The destination square (<see cref="Move.To"/>)
    /// - Any side effect moves (<see cref="Move.SideEffects"/>), appended after a '-' separator,
    ///   recursively encoded with the same format
    /// - Any explicitly captured squares (<see cref="Move.CapturedSquares"/>), appended after "!"
    ///
    /// Example encoding for castling with rook side effect and a capture:
    /// <code>e1>f1g1-h1f1!d4</code>
    /// </summary>
    public IEnumerable<string> EncodeLegalMoves(IEnumerable<Move> legalMoves) =>
        legalMoves.Select(EncodeMoveFlat);

    private static string EncodeMoveFlat(Move move)
    {
        var path = new StringBuilder();
        var captures = new List<Point>();
        BuildPath(move, path, captures);

        foreach (var capture in captures)
        {
            path.Append('!');
            path.Append(capture.AsUCI());
        }

        return path.ToString();
    }

    private static void BuildPath(Move move, StringBuilder path, List<Point> captures)
    {
        path.Append(move.From.AsUCI());
        foreach (var throughPoint in move.Through ?? [])
            path.Append(throughPoint.AsUCI());
        path.Append(move.To.AsUCI());

        if (move.CapturedSquares != null)
            captures.AddRange(move.CapturedSquares);

        foreach (var sideEffect in move.SideEffects ?? [])
        {
            path.Append('-');
            BuildPath(sideEffect, path, captures);
        }
    }
}
