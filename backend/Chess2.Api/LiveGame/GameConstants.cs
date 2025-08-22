using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.LiveGame;

public static class GameConstants
{
    public static readonly Dictionary<AlgebraicPoint, Piece> StartingPosition = new()
    {
        #region White Pieces
        [new AlgebraicPoint("a1")] = new Piece(PieceType.Rook, GameColor.White),
        [new AlgebraicPoint("b1")] = new Piece(PieceType.Horsey, GameColor.White),
        [new AlgebraicPoint("c1")] = new Piece(PieceType.Knook, GameColor.White),
        //[new AlgebraicPoint("d1")] = new Piece(PieceType.Xook, GameColor.White),
        [new AlgebraicPoint("e1")] = new Piece(PieceType.Queen, GameColor.White),
        [new AlgebraicPoint("f1")] = new Piece(PieceType.King, GameColor.White),
        [new AlgebraicPoint("g1")] = new Piece(PieceType.Bishop, GameColor.White),
        [new AlgebraicPoint("h1")] = new Piece(PieceType.Antiqueen, GameColor.White),
        [new AlgebraicPoint("i1")] = new Piece(PieceType.Horsey, GameColor.White),
        [new AlgebraicPoint("j1")] = new Piece(PieceType.Rook, GameColor.White),

        [new AlgebraicPoint("a2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("b2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("c2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("d2")] = new Piece(PieceType.UnderagePawn, GameColor.White),
        [new AlgebraicPoint("e2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("f2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("g2")] = new Piece(PieceType.UnderagePawn, GameColor.White),
        [new AlgebraicPoint("h2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("i2")] = new Piece(PieceType.Pawn, GameColor.White),
        [new AlgebraicPoint("j2")] = new Piece(PieceType.Pawn, GameColor.White),
        #endregion

        #region Black Pieces
        [new AlgebraicPoint("a9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("b9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("c9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("d9")] = new Piece(PieceType.UnderagePawn, GameColor.Black),
        [new AlgebraicPoint("e9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("f9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("g9")] = new Piece(PieceType.UnderagePawn, GameColor.Black),
        [new AlgebraicPoint("h9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("i9")] = new Piece(PieceType.Pawn, GameColor.Black),
        [new AlgebraicPoint("j9")] = new Piece(PieceType.Pawn, GameColor.Black),

        [new AlgebraicPoint("a10")] = new Piece(PieceType.Rook, GameColor.Black),
        [new AlgebraicPoint("b10")] = new Piece(PieceType.Horsey, GameColor.Black),
        [new AlgebraicPoint("c10")] = new Piece(PieceType.Knook, GameColor.Black),
        //[new AlgebraicPoint("d10")] = new Piece(PieceType.Xook, GameColor.Black),
        [new AlgebraicPoint("e10")] = new Piece(PieceType.Queen, GameColor.Black),
        [new AlgebraicPoint("f10")] = new Piece(PieceType.King, GameColor.Black),
        [new AlgebraicPoint("g10")] = new Piece(PieceType.Bishop, GameColor.Black),
        [new AlgebraicPoint("h10")] = new Piece(PieceType.Antiqueen, GameColor.Black),
        [new AlgebraicPoint("i10")] = new Piece(PieceType.Horsey, GameColor.Black),
        [new AlgebraicPoint("j10")] = new Piece(PieceType.Rook, GameColor.Black),
        #endregion
    };

    public const int BoardWidth = 10;
    public const int BoardHeight = 10;
}
