using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public abstract class PawnLikeTestData : TheoryData<PieceTestCase>
{
    protected void AddRegularMoveTests(PieceType pawnType, int maxInitialMoveDistance)
    {
        var whitePawn = PieceFactory.White(pawnType, timesMoved: 0);
        var blackPawn = PieceFactory.Black(pawnType, timesMoved: 0);
        var movedWhitePawn = PieceFactory.White(pawnType, timesMoved: 1);
        var movedBlackPawn = PieceFactory.Black(pawnType, timesMoved: 1);

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
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("f5")
                .GoesTo("e5")
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("white pawn captures diagonally both left and right")
        );

        Add(
            PieceTestCase
                .From("e6", movedBlackPawn)
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("f5")
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
                .WithFriendlyPieceAt("d5")
                .WithFriendlyPieceAt("f5")
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
                .WithEnemyPieceAt("f5")
                .GoesTo("e5")
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("white pawn can only capture diagonally to the right")
        );

        Add(
            PieceTestCase
                .From("e6", movedBlackPawn)
                .WithEnemyPieceAt("d5")
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
                .WithFriendlyPieceAt("e4")
                .GoesTo("e3")
                .WithDescription("white pawn is blocked on the first move")
        );

        Add(
            PieceTestCase
                .From("e9", blackPawn)
                .WithEnemyPieceAt("e7")
                .GoesTo("e8")
                .WithDescription("black pawn is blocked on the first move")
        );

        Add(
            PieceTestCase
                .From("e2", whitePawn)
                .WithEnemyPieceAt("e3")
                .WithEnemyPieceAt("d3")
                .WithEnemyPieceAt("f3")
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("f3", captures: ["f3"])
                .WithDescription(
                    "white pawn captures diagonally on first move with forward blocked"
                )
        );

        Add(
            PieceTestCase
                .From("e9", blackPawn)
                .WithEnemyPieceAt("e8")
                .WithEnemyPieceAt("d8")
                .WithEnemyPieceAt("f8")
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
                .WithPriorMove(from: "d9", to: "d7")
                .GoesTo(
                    "d8",
                    captures: ["d7"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .GoesTo("e8")
                .WithDescription("white pawn can capture en passant")
        );

        Add(
            PieceTestCase
                .From("e4", movedBlackPawn)
                .WithPieceAt("f2", whitePawn)
                .WithPriorMove(from: "f2", to: "f4")
                .GoesTo(
                    "f3",
                    captures: ["f4"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .GoesTo("e3")
                .WithDescription("black pawn can capture en passant")
        );

        Add(
            PieceTestCase
                .From("e6", movedWhitePawn)
                .WithPieceAt("d9", blackPawn)
                .WithEnemyPieceAt("c7")
                .WithEnemyPieceAt("b8")
                .WithPriorMove(from: "d9", to: "d6")
                .GoesTo("e7")
                .GoesTo(
                    "d7",
                    captures: ["d6"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .GoesTo(
                    "c8",
                    captures: ["d6", "c7"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .GoesTo(
                    "b9",
                    captures: ["d6", "c7", "b8"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .WithDescription("white pawn can capture long passant")
        );

        Add(
            PieceTestCase
                .From("e5", movedBlackPawn)
                .WithPieceAt("f2", whitePawn)
                .WithEnemyPieceAt("g4")
                .WithEnemyPieceAt("h3")
                .WithPriorMove(from: "f2", to: "f5")
                .GoesTo("e4")
                .GoesTo(
                    "f4",
                    captures: ["f5"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .GoesTo(
                    "g3",
                    captures: ["f5", "g4"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .GoesTo(
                    "h2",
                    captures: ["f5", "g4", "h3"],
                    forcedPriority: ForcedMovePriority.EnPassant,
                    specialMoveType: SpecialMoveType.EnPassant
                )
                .WithDescription("black pawn can capture long passant")
        );
        #endregion
    }

    public void AddPromotionTests(PieceType pawnType)
    {
        var movedWhitePawn = PieceFactory.White(pawnType, timesMoved: 1);
        var movedBlackPawn = PieceFactory.Black(pawnType, timesMoved: 1);

        Add(
            PieceTestCase
                .From("f9", movedWhitePawn)
                .GoesTo("f10", promotesTo: PieceType.Queen)
                .GoesTo("f10", promotesTo: PieceType.Rook)
                .GoesTo("f10", promotesTo: PieceType.Bishop)
                .GoesTo("f10", promotesTo: PieceType.Horsey)
                .GoesTo("f10", promotesTo: PieceType.Knook)
                .GoesTo("f10", promotesTo: PieceType.Antiqueen)
                .GoesTo("f10", promotesTo: PieceType.Checker)
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
                .GoesTo("f1", promotesTo: PieceType.Checker)
                .WithDescription("black pawn can promote")
        );
    }
}
