using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public abstract class BasePawnDefinitionTestData : TheoryData<PieceTestCase>
{
    protected void AddTestCases(PieceType pawnType, int maxInitialMoveDistance)
    {
        var whitePawn = PieceFactory.White(pawnType);
        var blackPawn = PieceFactory.Black(pawnType);
        var movedWhitePawn = PieceFactory.White(pawnType, timesMoved: 1);
        var movedBlackPawn = PieceFactory.Black(pawnType, timesMoved: 1);

        var whitePiece = PieceFactory.White();
        var blackPiece = PieceFactory.Black();

        #region regular moves
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
                .WithPieceAt("d5", blackPiece)
                .WithPieceAt("f5", blackPiece)
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("white pawn captures diagonally both left and right")
        );

        Add(
            PieceTestCase
                .From("e6", movedBlackPawn)
                .WithPieceAt("d5", whitePiece)
                .WithPieceAt("f5", whitePiece)
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("black pawn captures diagonally both left and right")
        );

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .WithPieceAt("e5", movedBlackPawn)
                .WithDescription("white pawn blocked directly forward by enemy")
        );

        Add(
            PieceTestCase
                .From("e4", movedWhitePawn)
                .WithPieceAt("d5", whitePiece)
                .WithPieceAt("f5", whitePiece)
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
                .WithPieceAt("f5", blackPiece)
                .GoesTo("e5")
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("white pawn can only capture diagonally to the right")
        );

        Add(
            PieceTestCase
                .From("e6", movedBlackPawn)
                .WithPieceAt("d5", whitePiece)
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .WithDescription("black pawn can only capture diagonally to the left")
        );
        #endregion

        #region first move
        var forwardSquaresWhite = Enumerable
            .Range(1, maxInitialMoveDistance)
            .Select(i => $"e{2 + i}")
            .ToArray();

        var forwardSquaresBlack = Enumerable
            .Range(1, maxInitialMoveDistance)
            .Select(i => $"e{9 - i}")
            .ToArray();

        Add(
            PieceTestCase
                .From("e2", whitePawn)
                .GoesTo(forwardSquaresWhite)
                .WithDescription("white pawn can move multiple square on the first move")
        );

        Add(
            PieceTestCase
                .From("e9", blackPawn)
                .GoesTo(forwardSquaresBlack)
                .WithDescription("black pawn can move multiple square on the first move")
        );

        Add(
            PieceTestCase
                .From("e2", whitePawn)
                .WithPieceAt("e4", whitePiece)
                .GoesTo("e3")
                .WithDescription("white pawn is blocked on the first move")
        );

        Add(
            PieceTestCase
                .From("e9", blackPawn)
                .WithPieceAt("e7", whitePiece)
                .GoesTo("e8")
                .WithDescription("black pawn is blocked on the first move")
        );

        Add(
            PieceTestCase
                .From("e2", whitePawn)
                .WithPieceAt("e3", blackPiece)
                .WithPieceAt("d3", blackPiece)
                .WithPieceAt("f3", blackPiece)
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("f3", captures: ["f3"])
                .WithDescription(
                    "white pawn captures diagonally on first move with forward blocked"
                )
        );

        Add(
            PieceTestCase
                .From("e9", blackPawn)
                .WithPieceAt("e8", whitePiece)
                .WithPieceAt("d8", whitePiece)
                .WithPieceAt("f8", whitePiece)
                .GoesTo("d8", captures: ["d8"])
                .GoesTo("f8", captures: ["f8"])
                .WithDescription(
                    "black pawn captures diagonally on first move with forward blocked"
                )
        );
        #endregion

        #region en passant
        Add(
            PieceTestCase
                .From("e7", movedWhitePawn)
                .WithPieceAt("d9", blackPawn)
                .WithPriorMove(new Move(new("d9"), new("d7"), blackPawn))
                .GoesTo("d8", captures: ["d7"], forcedPriority: ForcedMovePriority.EnPassant)
                .GoesTo("e8")
                .WithDescription("white pawn can capture en passant")
        );

        Add(
            PieceTestCase
                .From("e4", movedBlackPawn)
                .WithPieceAt("f2", whitePawn)
                .WithPriorMove(new Move(new("f2"), new("f4"), whitePawn))
                .GoesTo("f3", captures: ["f4"], forcedPriority: ForcedMovePriority.EnPassant)
                .GoesTo("e3")
                .WithDescription("black pawn can capture en passant")
        );

        Add(
            PieceTestCase
                .From("e6", movedWhitePawn)
                .WithPieceAt("d9", blackPawn)
                .WithPieceAt("c7", blackPiece)
                .WithPieceAt("b8", blackPiece)
                .WithPriorMove(new Move(new("d9"), new("d6"), blackPawn))
                .GoesTo("e7")
                .GoesTo("d7", captures: ["d6"], forcedPriority: ForcedMovePriority.EnPassant)
                .GoesTo("c8", captures: ["d6", "c7"], forcedPriority: ForcedMovePriority.EnPassant)
                .GoesTo(
                    "b9",
                    captures: ["d6", "c7", "b8"],
                    forcedPriority: ForcedMovePriority.EnPassant
                )
                .WithDescription("white pawn can capture long passant")
        );

        Add(
            PieceTestCase
                .From("e5", movedBlackPawn)
                .WithPieceAt("f2", whitePawn)
                .WithPieceAt("g4", whitePiece)
                .WithPieceAt("h3", whitePiece)
                .WithPriorMove(new Move(new("f2"), new("f5"), whitePawn))
                .GoesTo("e4")
                .GoesTo("f4", captures: ["f5"], forcedPriority: ForcedMovePriority.EnPassant)
                .GoesTo("g3", captures: ["f5", "g4"], forcedPriority: ForcedMovePriority.EnPassant)
                .GoesTo(
                    "h2",
                    captures: ["f5", "g4", "h3"],
                    forcedPriority: ForcedMovePriority.EnPassant
                )
                .WithDescription("black pawn can capture en passant")
        );
        #endregion

        #region promotion
        Add(
            PieceTestCase
                .From("f9", movedWhitePawn)
                .GoesTo("f10", promotesTo: PieceType.Queen)
                .GoesTo("f10", promotesTo: PieceType.Rook)
                .GoesTo("f10", promotesTo: PieceType.Bishop)
                .GoesTo("f10", promotesTo: PieceType.Horsey)
                .GoesTo("f10", promotesTo: PieceType.Knook)
                .GoesTo("f10", promotesTo: PieceType.Antiqueen)
                .WithDescription("white pawn can promote")
        );

        Add(
            PieceTestCase
                .From("f2", movedBlackPawn)
                .GoesTo("f1", promotesTo: PieceType.Queen)
                .GoesTo("f1", promotesTo: PieceType.Rook)
                .GoesTo("f1", promotesTo: PieceType.Bishop)
                .GoesTo("f1", promotesTo: PieceType.Horsey)
                .GoesTo("f1", promotesTo: PieceType.Knook)
                .GoesTo("f1", promotesTo: PieceType.Antiqueen)
                .WithDescription("black pawn can promote")
        );
        #endregion
    }
}
