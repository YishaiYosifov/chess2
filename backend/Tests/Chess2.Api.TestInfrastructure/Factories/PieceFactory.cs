using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Fakes;

namespace Chess2.Api.TestInfrastructure.Factories;

public static class PieceFactory
{
    public static Piece White(PieceType? type = null, int? timesMoved = null) =>
        new PieceFaker(GameColor.White, type, timesMoved).Generate();

    public static Piece Black(PieceType? type = null, int? timesMoved = null) =>
        new PieceFaker(GameColor.Black, type, timesMoved).Generate();

    public static Piece Neutral(PieceType? type = null, int? timesMoved = null) =>
        new PieceFaker(color: null, type, timesMoved).Generate();
}
