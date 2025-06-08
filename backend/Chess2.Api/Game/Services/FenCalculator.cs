using System.Text;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IFenCalculator
{
    string CalculateFen(ChessBoard board);
}

public class FenCalculator(IPieceToLetter pieceToLetter) : IFenCalculator
{
    private readonly IPieceToLetter _pieceToLetter = pieceToLetter;

    public string CalculateFen(ChessBoard board)
    {
        var sb = new StringBuilder();

        // enumerate from black perspective because we FENs start with the black pieces
        for (int y = board.Height - 1; y >= 0; y--)
        {
            int emptyCount = 0;
            for (int x = 0; x < board.Width; x++)
            {
                var point = new Point(x, y);
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
                pieceLetter =
                    piece.Color == GameColor.White ? pieceLetter.ToUpper() : pieceLetter.ToLower();

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
