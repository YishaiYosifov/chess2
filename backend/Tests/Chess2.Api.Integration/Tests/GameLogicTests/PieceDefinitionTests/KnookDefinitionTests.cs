using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class KnookDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(KnookDefinitionTestData))]
    public void KnookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class KnookDefinitionTestData : TheoryData<PieceTestCase>
{
    public KnookDefinitionTestData()
    {
        var knook = PieceFactory.White(PieceType.Knook);

        Add(
            PieceTestCase
                .From("e5", knook)
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
                // no horsey moves as there are no captures
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", knook)
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
                .WithDescription("Corner case: knook at a1")
        );

        Add(
            PieceTestCase
                .From("a5", knook)
                // vertical up
                .GoesTo("a6")
                .GoesTo("a7")
                .GoesTo("a8")
                .GoesTo("a9")
                .GoesTo("a10")
                // vertical down
                .GoesTo("a4")
                .GoesTo("a3")
                .GoesTo("a2")
                .GoesTo("a1")
                // horizontal right
                .GoesTo("b5")
                .GoesTo("c5")
                .GoesTo("d5")
                .GoesTo("e5")
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5")
                .GoesTo("i5")
                .GoesTo("j5")
                .WithDescription("Edge case: knook at a5")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithFriendlyPieceAt("e7") // blocks beyond e6
                .WithFriendlyPieceAt("h5") // blocks beyond g5
                .WithFriendlyPieceAt("f7") // blocks horsey part
                // vertical up
                .GoesTo("e6")
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
                .WithDescription("Blocked by friendly")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithFriendlyPieceAt("e6")
                .WithFriendlyPieceAt("e4")
                .WithFriendlyPieceAt("d5")
                .WithFriendlyPieceAt("f5")
                .WithFriendlyPieceAt("d7")
                .WithFriendlyPieceAt("f7")
                .WithFriendlyPieceAt("c6")
                .WithFriendlyPieceAt("g6")
                .WithFriendlyPieceAt("c4")
                .WithFriendlyPieceAt("g4")
                .WithFriendlyPieceAt("d3")
                .WithFriendlyPieceAt("f3")
                .WithDescription("Surrounded by friendly pieces in all directions")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithEnemyPieceAt("e6")
                .WithEnemyPieceAt("e4")
                .WithEnemyPieceAt("d5")
                .WithEnemyPieceAt("f5")
                .WithEnemyPieceAt("d7")
                .WithEnemyPieceAt("f7")
                .WithEnemyPieceAt("c6")
                .WithEnemyPieceAt("g6")
                .WithEnemyPieceAt("c4")
                .WithEnemyPieceAt("g4")
                .WithEnemyPieceAt("d3")
                .WithEnemyPieceAt("f3")
                // only horse movement can capture
                .GoesTo("d7", captures: ["d7"])
                .GoesTo("f7", captures: ["f7"])
                .GoesTo("c6", captures: ["c6"])
                .GoesTo("g6", captures: ["g6"])
                .GoesTo("c4", captures: ["c4"])
                .GoesTo("g4", captures: ["g4"])
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("f3", captures: ["f3"])
                .WithDescription("Surrounded by enemy pieces in all directions")
        );
    }
}
