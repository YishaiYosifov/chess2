using Chess2.Api.GameLogic.Errors;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.GameLogic;

public class ChessBoard
{
    private readonly Piece?[,] _board = new Piece?[10, 10];

    public ChessBoard(Dictionary<Point, Piece> pieces)
    {
        InitializeBoard(pieces);
    }

    private void InitializeBoard(Dictionary<Point, Piece> pieces)
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                _board[x, y] = pieces.GetValueOrDefault(new Point(x, y));
            }
        }
    }

    public Piece? GetAt(Point point) => _board[point.X, point.Y];

    public ErrorOr<Success> MovePiece(Point from, Point to)
    {
        var piece = GetAt(from);
        if (piece is null)
            return GameErrors.PieceNotFound;

        _board[from.X, from.Y] = null;
        _board[to.X, to.Y] = piece;
        return Result.Success;
    }
}
