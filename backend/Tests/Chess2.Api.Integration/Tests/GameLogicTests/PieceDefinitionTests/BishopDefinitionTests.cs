using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class BishopDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(BishopDefinitionTestData))]
    public void BishopDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class BishopDefinitionTestData : TheoryData<PieceTestCase>
{
    public BishopDefinitionTestData()
    {
        var bishop = PieceFactory.White(PieceType.Bishop);
        var friendyUnderagePawn = PieceFactory.White(PieceType.UnderagePawn);
        var enemyUnderagePawn = PieceFactory.Black(PieceType.UnderagePawn);

        string[] openE5Moves =
        [
            // diagonal up-left
            "d6",
            "c7",
            "b8",
            "a9",
            // diagonal up-right
            "f6",
            "g7",
            "h8",
            "i9",
            "j10",
            // diagonal down-left
            "d4",
            "c3",
            "b2",
            "a1",
            // diagonal down-right
            "f4",
            "g3",
            "h2",
            "i1",
        ];

        Add(
            PieceTestCase
                .From("e5", bishop)
                .GoesTo(openE5Moves)
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", bishop)
                // diagonal up-right only
                .GoesTo("b2")
                .GoesTo("c3")
                .GoesTo("d4")
                .GoesTo("e5")
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
                .WithDescription("Corner case: bishop at a1")
        );

        Add(
            PieceTestCase
                .From("a5", bishop)
                // diagonal up-right
                .GoesTo("b6")
                .GoesTo("c7")
                .GoesTo("d8")
                .GoesTo("e9")
                .GoesTo("f10")
                // diagonal down-right
                .GoesTo("b4")
                .GoesTo("c3")
                .GoesTo("d2")
                .GoesTo("e1")
                .WithDescription("Edge case: bishop at a5")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithFriendlyPieceAt("g7") // blocks beyond f6
                // diagonal up-left
                .GoesTo("d6")
                .GoesTo("c7")
                .GoesTo("b8")
                .GoesTo("a9")
                // diagonal up-right, stops before g7
                .GoesTo("f6")
                // cannot go to g7 (blocked by friendly)
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3")
                .GoesTo("b2")
                .GoesTo("a1")
                // diagonal down-right
                .GoesTo("f4")
                .GoesTo("g3")
                .GoesTo("h2")
                .GoesTo("i1")
                .WithDescription("Blocked by friendly piece in diagonal up-right direction")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithEnemyPieceAt("g3") // enemy can be captured, blocks beyond
                .WithFriendlyPieceAt("h2") // friendly beyond enemy
                // diagonal up-left
                .GoesTo("d6")
                .GoesTo("c7")
                .GoesTo("b8")
                .GoesTo("a9")
                // diagonal up-right
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3")
                .GoesTo("b2")
                .GoesTo("a1")
                // diagonal down-right
                .GoesTo("f4")
                .GoesTo("g3", captures: ["g3"])
                // cannot go beyond g3 because of enemy blocker
                .WithDescription("Blocked by enemy piece in diagonal down-right direction")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithFriendlyPieceAt("d6")
                .WithFriendlyPieceAt("f6")
                .WithFriendlyPieceAt("d4")
                .WithFriendlyPieceAt("f4")
                .WithDescription("Bishop surrounded by friendly pieces on all diagonals (no moves)")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithEnemyPieceAt("d6")
                .WithEnemyPieceAt("f6")
                .WithEnemyPieceAt("d4")
                .WithEnemyPieceAt("f4")
                .GoesTo("d6", captures: ["d6"])
                .GoesTo("f6", captures: ["f6"])
                .GoesTo("d4", captures: ["d4"])
                .GoesTo("f4", captures: ["f4"])
                .WithDescription(
                    "Bishop surrounded by enemy pieces on all diagonals (can capture all"
                )
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("c7", friendyUnderagePawn)
                .WithPieceAt("h8", friendyUnderagePawn)
                .WithPieceAt("a1", friendyUnderagePawn)
                .WithPieceAt("f4", friendyUnderagePawn)
                // diagonal up-left
                .GoesTo("d6")
                .GoesTo("c7", captures: ["c7"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal up-right
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8", captures: ["h8"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3")
                .GoesTo("b2")
                .GoesTo("a1", captures: ["a1"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-right
                .GoesTo("f4", captures: ["f4"], forcedPriority: ForcedMovePriority.UnderagePawn)
                .WithDescription("Forced friendly underage pawn capture")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("a9", enemyUnderagePawn)
                .WithPieceAt("f6", enemyUnderagePawn)
                .WithPieceAt("b2", enemyUnderagePawn)
                .WithPieceAt("g3", enemyUnderagePawn)
                // diagonal up-left
                .GoesTo("d6")
                .GoesTo("c7")
                .GoesTo("b8")
                .GoesTo("a9", captures: ["a9"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal up-right
                .GoesTo("f6", captures: ["f6"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-left
                .GoesTo("d4")
                .GoesTo("c3")
                .GoesTo("b2", captures: ["b2"], forcedPriority: ForcedMovePriority.UnderagePawn)
                // diagonal down-right
                .GoesTo("f4")
                .GoesTo("g3", captures: ["g3"], forcedPriority: ForcedMovePriority.UnderagePawn)
                .WithDescription("Forced enemy underage pawn capture")
        );

        var partnerIlVaticano = PieceFactory.White(PieceType.Bishop);
        var resgularEnemy = PieceFactory.Black(PieceType.Rook);
        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("h5", partnerIlVaticano)
                .WithPieceAt("f5", resgularEnemy)
                .WithPieceAt("g5", resgularEnemy)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "h5",
                    trigger: ["f5", "g5"],
                    captures: ["f5", "g5"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("h5"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano right moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("b5", partnerIlVaticano)
                .WithPieceAt("c5", resgularEnemy)
                .WithPieceAt("d5", resgularEnemy)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "b5",
                    trigger: ["c5", "d5"],
                    captures: ["c5", "d5"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("b5"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano left moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("e8", partnerIlVaticano)
                .WithPieceAt("e7", resgularEnemy)
                .WithPieceAt("e6", resgularEnemy)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "e8",
                    trigger: ["e6", "e7"],
                    captures: ["e6", "e7"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("e8"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano up moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("e2", partnerIlVaticano)
                .WithPieceAt("e3", resgularEnemy)
                .WithPieceAt("e4", resgularEnemy)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "e2",
                    trigger: ["e4", "e3"],
                    captures: ["e4", "e3"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("e2"), To: new("e5"), partnerIlVaticano),
                    ],
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Il vaticano down moves")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("h5", partnerIlVaticano)
                .WithPieceAt("f5", resgularEnemy)
                .WithPieceAt("g5", enemyUnderagePawn)
                .GoesTo(openE5Moves)
                .GoesTo(
                    "h5",
                    trigger: ["f5", "g5"],
                    captures: ["f5", "g5"],
                    sideEffects:
                    [
                        new MoveSideEffect(From: new("h5"), To: new("e5"), partnerIlVaticano),
                    ],
                    forcedPriority: ForcedMovePriority.UnderagePawn,
                    specialMoveType: SpecialMoveType.IlVaticano
                )
                .WithDescription("Forced il vaticano with underage pawn")
        );
    }
}
