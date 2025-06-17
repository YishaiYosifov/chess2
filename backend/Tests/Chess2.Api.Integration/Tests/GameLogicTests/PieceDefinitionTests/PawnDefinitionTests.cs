using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class PawnDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(PawnDefinitionTestData))]
    public void PawnDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class PawnDefinitionTestData : TheoryData<PieceTestCase>
{
    public PawnDefinitionTestData()
    {
        var movedWhitePawn = PieceFactory.White(PieceType.Pawn, timesMoved: 1);
        var movedBlackPawn = PieceFactory.Black(PieceType.Pawn, timesMoved: 1);

        var whitePiece = PieceFactory.White();
        var blackPiece = PieceFactory.Black();

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .GoesTo("e5")
                .WithDescription("white pawn moves one step forward from e4")
        );

        Add(
            PieceTestCase
                .From("e7", movedBlackPawn)
                .GoesTo("e6")
                .WithDescription("black pawn moves one step forward from e7")
        );

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .WithBlocker("d5", blackPiece)
                .WithBlocker("f5", blackPiece)
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("white pawn captures diagonally both left and right")
        );

        Add(
            PieceTestCase
                .From("e6", movedBlackPawn)
                .WithBlocker("d5", whitePiece)
                .WithBlocker("f5", whitePiece)
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("black pawn captures diagonally both left and right")
        );

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .WithBlocker("e5", movedBlackPawn)
                .WithDescription("white pawn blocked directly forward by enemy")
        );

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .WithBlocker("d5", whitePiece)
                .WithBlocker("f5", whitePiece)
                .GoesTo("e5")
                .WithDescription("white pawn cannot capture friendly pieces diagonally")
        );

        Add(
            PieceTestCase
                .From("e10", movedWhitePawn)
                .WithDescription("white pawn at top of board has no forward moves")
        );

        Add(
            PieceTestCase
                .From("e1", movedBlackPawn)
                .WithDescription("black pawn at bottom of board has no forward moves")
        );

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .WithBlocker("f5", blackPiece)
                .GoesTo("e5")
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("white pawn can only capture diagonally to the right")
        );

        Add(
            PieceTestCase
                .From("e6", movedBlackPawn)
                .WithBlocker("d5", whitePiece)
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .WithDescription("black pawn can only capture diagonally to the left")
        );

        Add(
            PieceTestCase
                .From("e2", PieceFactory.White(PieceType.Pawn, timesMoved: 0))
                .GoesTo("e3")
                .GoesTo("e4")
                .GoesTo("e5")
                .WithDescription("white pawn can move multiple square on the first move")
        );

        Add(
            PieceTestCase
                .From("e9", PieceFactory.Black(PieceType.Pawn, timesMoved: 0))
                .GoesTo("e8")
                .GoesTo("e7")
                .GoesTo("e6")
                .WithDescription("black pawn can move multiple square on the first move")
        );

        Add(
            PieceTestCase
                .From("e2", PieceFactory.White(PieceType.Pawn, timesMoved: 0))
                .WithBlocker("e4", whitePiece)
                .GoesTo("e3")
                .WithDescription("white pawn is blocked on the first move")
        );

        Add(
            PieceTestCase
                .From("e9", PieceFactory.Black(PieceType.Pawn, timesMoved: 0))
                .WithBlocker("e7", whitePiece)
                .GoesTo("e8")
                .WithDescription("black pawn is blocked on the first move")
        );

        Add(
            PieceTestCase
                .From("e2", PieceFactory.White(PieceType.Pawn, timesMoved: 0))
                .WithBlocker("e3", blackPiece)
                .WithBlocker("d3", blackPiece)
                .WithBlocker("f3", blackPiece)
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("f3", captures: ["f3"])
                .WithDescription(
                    "white pawn captures diagonally on first move with forward blocked"
                )
        );

        Add(
            PieceTestCase
                .From("e9", PieceFactory.Black(PieceType.Pawn, timesMoved: 0))
                .WithBlocker("e8", whitePiece)
                .WithBlocker("d8", whitePiece)
                .WithBlocker("f8", whitePiece)
                .GoesTo("d8", captures: ["d8"])
                .GoesTo("f8", captures: ["f8"])
                .WithDescription(
                    "black pawn captures diagonally on first move with forward blocked"
                )
        );

        Add(
            PieceTestCase
                .From("e7", movedWhitePawn)
                .WithBlocker("d9", movedBlackPawn)
                .WithPriorMove(new Move(new("d9"), new("d7"), movedBlackPawn))
                .GoesTo("d8", captures: ["d7"])
                .GoesTo("e8")
                .WithDescription("white pawn can capture en passant")
        );

        Add(
            PieceTestCase
                .From("e4", movedBlackPawn)
                .WithBlocker("f2", movedWhitePawn)
                .WithPriorMove(new Move(new("f2"), new("f4"), movedWhitePawn))
                .GoesTo("f3", captures: ["f4"])
                .GoesTo("e3")
                .WithDescription("black pawn can capture en passant")
        );
    }
}
