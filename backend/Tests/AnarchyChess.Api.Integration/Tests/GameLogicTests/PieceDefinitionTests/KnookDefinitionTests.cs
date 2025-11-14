using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Utils;

namespace AnarchyChess.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class KnookDefinitionTests(AnarchyChessWebApplicationFactory factory)
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
                // up
                .GoesTo("e6")
                .GoesTo("e7")
                // down
                .GoesTo("e4")
                .GoesTo("e3")
                // left
                .GoesTo("d5")
                .GoesTo("c5")
                // right
                .GoesTo("f5")
                .GoesTo("g5")
                // horsey
                .GoesTo("d7")
                .GoesTo("f7")
                .GoesTo("c6")
                .GoesTo("g6")
                .GoesTo("c4")
                .GoesTo("g4")
                .GoesTo("d3")
                .GoesTo("f3")
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", knook)
                // up
                .GoesTo("a2")
                .GoesTo("a3")
                // right
                .GoesTo("b1")
                .GoesTo("c1")
                // horsey
                .GoesTo("b3")
                .GoesTo("c2")
                .WithDescription("Corner case: knook at a1")
        );

        Add(
            PieceTestCase
                .From("a5", knook)
                // up
                .GoesTo("a6")
                .GoesTo("a7")
                // down
                .GoesTo("a4")
                .GoesTo("a3")
                // right
                .GoesTo("b5")
                .GoesTo("c5")
                // horsey
                .GoesTo("b7")
                .GoesTo("c6")
                .GoesTo("c4")
                .GoesTo("b3")
                .WithDescription("Edge case: knook at a5")
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
                .GoesTo("e6", captures: ["e6"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
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
