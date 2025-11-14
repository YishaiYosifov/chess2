using System.Text;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Game.Services;

public interface IFenCalculator
{
    string CalculateFen(IReadOnlyChessBoard board);
}

public class FenCalculator(IPieceToLetter pieceToLetter) : IFenCalculator
{
    private readonly IPieceToLetter _pieceToLetter = pieceToLetter;

    public string CalculateFen(IReadOnlyChessBoard board)
    {
        var sb = new StringBuilder();

        // enumerate from black perspective because we FENs start with the black pieces
        for (int y = board.Height - 1; y >= 0; y--)
        {
            int emptyCount = 0;
            for (int x = 0; x < board.Width; x++)
            {
                var point = new AlgebraicPoint(x, y);
                if (!board.TryGetPieceAt(point, out var piece))
                {
                    emptyCount++;
                    continue;
                }

                if (emptyCount > 0)
                {
                    sb.Append(emptyCount);
                    emptyCount = 0;
                }

                var pieceLetter = _pieceToLetter.GetLetter(piece.Type);
                pieceLetter = piece.Color.Match(
                    whenWhite: pieceLetter.ToUpper(),
                    whenBlack: pieceLetter.ToLower(),
                    whenNeutral: pieceLetter
                );

                sb.Append(pieceLetter);
            }

            if (emptyCount > 0)
                sb.Append(emptyCount);
            if (y > 0)
                sb.Append('/');
        }

        var fen = sb.ToString();
        return fen;
    }
}
