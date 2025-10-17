using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class QueenDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(QueenDefinitionTestData))]
    public void QueenDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class QueenDefinitionTestData : TheoryData<PieceTestCase>
{
    public QueenDefinitionTestData()
    {
        var queen = PieceFactory.White(PieceType.Queen);
        Console.WriteLine(queen.TimesMoved);

        Add(
            PieceTestCase
                .From("e5", queen)
                // vertical up
                .GoesTo("e6", "e7", "e8", "e9", "e10")
                // vertical down
                .GoesTo("e4", "e3", "e2", "e1")
                // horizontal left
                .GoesTo("d5", "c5", "b5", "a5")
                // horizontal right
                .GoesTo("f5", "g5", "h5", "i5", "j5")
                .GoesTo(
                    "e5",
                    spawns:
                    [
                        new PieceSpawn(PieceType.SterilePawn, GameColor.White, new("j5")),
                        new PieceSpawn(PieceType.Bishop, GameColor.White, new("i5")),
                        new PieceSpawn(PieceType.Rook, GameColor.White, new("h5")),
                    ],
                    captures: ["e5"],
                    specialMoveType: SpecialMoveType.RadioactiveBetaDecay
                ) // radioactive beta decay
                // diagonal up-left
                .GoesTo("d6", "c7", "b8", "a9")
                // diagonal up-right
                .GoesTo("f6", "g7", "h8", "i9", "j10")
                // diagonal down-left
                .GoesTo("d4", "c3", "b2", "a1")
                // diagonal down-right
                .GoesTo("f4", "g3", "h2", "i1")
                .WithDescription("Open board from e5 (right decay)")
        );

        Add(
            PieceTestCase
                .From("f5", queen)
                // vertical up
                .GoesTo("f6", "f7", "f8", "f9", "f10")
                // vertical down
                .GoesTo("f4", "f3", "f2", "f1")
                // horizontal left
                .GoesTo("e5", "d5", "c5", "b5", "a5")
                .GoesTo(
                    "f5",
                    spawns:
                    [
                        new PieceSpawn(PieceType.SterilePawn, GameColor.White, new("a5")),
                        new PieceSpawn(PieceType.Bishop, GameColor.White, new("b5")),
                        new PieceSpawn(PieceType.Rook, GameColor.White, new("c5")),
                    ],
                    captures: ["f5"],
                    specialMoveType: SpecialMoveType.RadioactiveBetaDecay
                ) // radioactive beta decay
                // horizontal right
                .GoesTo("g5", "h5", "i5", "j5")
                // diagonal up-left
                .GoesTo("e6", "d7", "c8", "b9", "a10")
                // diagonal up-right
                .GoesTo("g6", "h7", "i8", "j9")
                // diagonal down-left
                .GoesTo("e4", "d3", "c2", "b1")
                // diagonal down-right
                .GoesTo("g4", "h3", "i2", "j1")
                .WithDescription("Open board from f5 (left decay)")
        );

        Add(
            PieceTestCase
                .From("a1", queen)
                // vertical up
                .GoesTo("a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "a10")
                // horizontal right
                .GoesTo("b1", "c1", "d1", "e1", "f1", "g1", "h1", "i1", "j1")
                .GoesTo(
                    "a1",
                    spawns:
                    [
                        new PieceSpawn(PieceType.SterilePawn, GameColor.White, new("j1")),
                        new PieceSpawn(PieceType.Bishop, GameColor.White, new("i1")),
                        new PieceSpawn(PieceType.Rook, GameColor.White, new("h1")),
                    ],
                    captures: ["a1"],
                    specialMoveType: SpecialMoveType.RadioactiveBetaDecay
                ) // radioactive beta decay
                // diagonal up-right
                .GoesTo("b2", "c3", "d4", "e5", "f6", "g7", "h8", "i9", "j10")
                .WithDescription("Queen in corner a1")
        );

        Add(
            PieceTestCase
                .From("a5", queen)
                .WithFriendlyPieceAt("a7") // friendly above, blocks beyond a6 vertical up
                .WithEnemyPieceAt("c5") // enemy right side, can capture at c5 but no further right
                // vertical up
                .GoesTo("a6")
                // horizontal right
                .GoesTo("b5")
                .GoesTo("c5", captures: ["c5"])
                // vertical down
                .GoesTo("a4", "a3", "a2", "a1")
                // diagonal up-right
                .GoesTo("b6", "c7", "d8", "e9", "f10")
                // diagonal down-right
                .GoesTo("b4", "c3", "d2", "e1")
                .WithDescription("Queen on edge a5 with blockers")
        );

        Add(
            PieceTestCase
                .From("e5", queen)
                .WithFriendlyPieceAt("e6")
                .WithFriendlyPieceAt("e4")
                .WithFriendlyPieceAt("d5")
                .WithFriendlyPieceAt("f5")
                .WithFriendlyPieceAt("d6")
                .WithFriendlyPieceAt("f6")
                .WithFriendlyPieceAt("d4")
                .WithFriendlyPieceAt("f4")
                .WithDescription("Queen surrounded by friendly pieces - no moves")
        );

        Add(
            PieceTestCase
                .From("e5", queen)
                .WithEnemyPieceAt("e6")
                .WithEnemyPieceAt("e4")
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("f5")
                .WithEnemyPieceAt("d6")
                .WithEnemyPieceAt("f6")
                .WithEnemyPieceAt("d4")
                .WithEnemyPieceAt("f4")
                .GoesTo("e6", captures: ["e6"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .GoesTo("d6", captures: ["d6"])
                .GoesTo("f6", captures: ["f6"])
                .GoesTo("d4", captures: ["d4"])
                .GoesTo("f4", captures: ["f4"])
                .WithDescription("Queen surrounded by enemy pieces - all moves are captures")
        );
    }
}
