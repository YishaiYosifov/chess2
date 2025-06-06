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

        int distanceWithNoPiece = 0;
        // enumerate from black perspective because we FENs start with the black pieces
        for (int y = board.Height; y >= 0; y--)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var point = new Point(x, y);
                if (!board.TryGetPieceAt(point, out var piece))
                {
                    distanceWithNoPiece++;
                    continue;
                }

                if (distanceWithNoPiece > 0)
                    sb.Append(y);
                sb.Append(_pieceToLetter.ToLetter(piece.Type));
            }

            if (distanceWithNoPiece > 0)
                sb.Append(y);
            sb.Append('/');
        }

        var fen = sb.ToString();
        return fen;
    }
}
