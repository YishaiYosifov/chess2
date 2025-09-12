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
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                // vertical down
                .GoesTo("e4")
                .GoesTo("e3")
                .GoesTo("e2")
                .GoesTo("e1")
                // horizontal left
                .GoesTo("d5")
                .GoesTo("c5")
                .GoesTo("b5")
                .GoesTo("a5")
                // horizontal right
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5")
                .GoesTo("i5")
                .GoesTo("j5")
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
                .GoesTo("g3")
                .GoesTo("h2")
                .GoesTo("i1")
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", queen)
                // vertical up
                .GoesTo("a2")
                .GoesTo("a3")
                .GoesTo("a4")
                .GoesTo("a5")
                .GoesTo("a6")
                .GoesTo("a7")
                .GoesTo("a8")
                .GoesTo("a9")
                .GoesTo("a10")
                // horizontal right
                .GoesTo("b1")
                .GoesTo("c1")
                .GoesTo("d1")
                .GoesTo("e1")
                .GoesTo("f1")
                .GoesTo("g1")
                .GoesTo("h1")
                .GoesTo("i1")
                .GoesTo("j1")
                // diagonal up-right
                .GoesTo("b2")
                .GoesTo("c3")
                .GoesTo("d4")
                .GoesTo("e5")
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
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
                .GoesTo("a4")
                .GoesTo("a3")
                .GoesTo("a2")
                .GoesTo("a1")
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
                // vertical up
                .GoesTo("e6", captures: ["e6"])
                // vertical down
                .GoesTo("e4", captures: ["e4"])
                // horizontal left
                .GoesTo("d5", captures: ["d5"])
                // horizontal right
                .GoesTo("f5", captures: ["f5"])
                // diagonal up-left
                .GoesTo("d6", captures: ["d6"])
                // diagonal up-right
                .GoesTo("f6", captures: ["f6"])
                // diagonal down-left
                .GoesTo("d4", captures: ["d4"])
                // diagonal down-right
                .GoesTo("f4", captures: ["f4"])
                .WithDescription("Queen surrounded by enemy pieces - all moves are captures")
        );
    }
}
