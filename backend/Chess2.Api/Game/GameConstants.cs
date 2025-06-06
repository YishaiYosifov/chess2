using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game;

public static class GameConstants
{
    public static readonly Dictionary<Point, Piece> StartingPosition = new()
    {
        #region White Pieces
        [new Point(0, 0)] = new Piece(PieceType.Rook, PieceColor.White),
        [new Point(1, 0)] = new Piece(PieceType.Horsey, PieceColor.White),
        //[new Point(2, 0)] = new Piece(PieceType.Knook, PieceColor.White),
        //[new Point(3, 0)] = new Piece(PieceType.Xook, PieceColor.White),
        [new Point(4, 0)] = new Piece(PieceType.Queen, PieceColor.White),
        [new Point(5, 0)] = new Piece(PieceType.King, PieceColor.White),
        [new Point(6, 0)] = new Piece(PieceType.Bishop, PieceColor.White),
        //[new Point(7, 0)] = new Piece(PieceType.Antiqueen, PieceColor.White),
        [new Point(8, 0)] = new Piece(PieceType.Horsey, PieceColor.White),
        [new Point(9, 0)] = new Piece(PieceType.Rook, PieceColor.White),

        [new Point(0, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        [new Point(1, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        //[new Point(2, 1)] = new Piece(PieceType.ChildPawn, PieceColor.White),
        [new Point(3, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        [new Point(4, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        [new Point(5, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        [new Point(6, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        //[new Point(6, 1)] = new Piece(PieceType.ChildPawn, PieceColor.White),
        [new Point(8, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        [new Point(9, 1)] = new Piece(PieceType.Pawn, PieceColor.White),
        #endregion

        #region Black Pieces
        [new Point(0, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        [new Point(1, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        //[new Point(2, 8)] = new Piece(PieceType.ChildPawn, PieceColor.Black),
        [new Point(3, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        [new Point(4, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        [new Point(5, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        [new Point(6, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        //[new Point(7, 8)] = new Piece(PieceType.ChildPawn, PieceColor.Black),
        [new Point(8, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),
        [new Point(9, 8)] = new Piece(PieceType.Pawn, PieceColor.Black),

        [new Point(0, 9)] = new Piece(PieceType.Rook, PieceColor.Black),
        [new Point(1, 9)] = new Piece(PieceType.Horsey, PieceColor.Black),
        //[new Point(2, 9)] = new Piece(PieceType.Knook, PieceColor.Black),
        //[new Point(3, 9)] = new Piece(PieceType.Xook, PieceColor.Black),
        [new Point(4, 9)] = new Piece(PieceType.Queen, PieceColor.Black),
        [new Point(5, 9)] = new Piece(PieceType.King, PieceColor.Black),
        [new Point(6, 9)] = new Piece(PieceType.Bishop, PieceColor.Black),
        //[new Point(7, 9)] = new Piece(PieceType.Antiqueen, PieceColor.Black),
        [new Point(8, 9)] = new Piece(PieceType.Horsey, PieceColor.Black),
        [new Point(9, 9)] = new Piece(PieceType.Rook, PieceColor.Black),
        #endregion
    };

    public const int BoardWidth = 10;
    public const int BoardHeight = 10;
}
