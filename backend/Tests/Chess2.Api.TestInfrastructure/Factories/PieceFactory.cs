using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Factories;

public static class PieceFactory
{
    public static Piece White(PieceType type = PieceType.Pawn, int timesMoved = 0) =>
        new(type, GameColor.White, TimesMoved: timesMoved);

    public static Piece Black(PieceType type = PieceType.Pawn, int timesMoved = 0) =>
        new(type, GameColor.Black, TimesMoved: timesMoved);

    public static Piece Neutral(PieceType type = PieceType.Pawn, int timesMoved = 0) =>
        new(type, null, TimesMoved: timesMoved);
}
