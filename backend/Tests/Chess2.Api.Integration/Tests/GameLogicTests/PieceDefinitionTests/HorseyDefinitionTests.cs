using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class HorseyDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(HorseyDefinitionTestData))]
    public void HorseyDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class HorseyDefinitionTestData : TheoryData<PieceTestCase>
{
    public HorseyDefinitionTestData()
    {
        var horsey = PieceFactory.White(PieceType.Horsey);

        Add(
            PieceTestCase
                .From("e5", horsey)
                // 8 possible L-shaped moves
                .GoesTo("d7") // up 2, left 1
                .GoesTo("f7") // up 2, right 1
                .GoesTo("c6") // up 1, left 2
                .GoesTo("g6") // up 1, right 2
                .GoesTo("c4") // down 1, left 2
                .GoesTo("g4") // down 1, right 2
                .GoesTo("d3") // down 2, left 1
                .GoesTo("f3") // down 2, right 1
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", horsey)
                .GoesTo("b3") // up 2, right 1
                .GoesTo("c2") // up 1, right 2
                .WithDescription("Corner case: horsey at a1")
        );

        Add(
            PieceTestCase
                .From("j10", horsey)
                .GoesTo("h9") // down 1, left 2
                .GoesTo("i8") // down 2, left 1
                .WithDescription("Corner case: horsey at j10")
        );

        Add(
            PieceTestCase
                .From("a5", horsey)
                .GoesTo("b7") // up 2, right 1
                .GoesTo("c6") // up 1, right 2
                .GoesTo("c4") // down 1, right 2
                .GoesTo("b3") // down 2, right 1
                .WithDescription("Edge case: horsey at a5")
        );

        Add(
            PieceTestCase
                .From("e5", horsey)
                .WithFriendlyPieceAt("d7")
                .WithFriendlyPieceAt("f7")
                .WithFriendlyPieceAt("c6")
                .WithFriendlyPieceAt("g6")
                .WithFriendlyPieceAt("c4")
                .WithFriendlyPieceAt("g4")
                .WithFriendlyPieceAt("d3")
                .WithFriendlyPieceAt("f3")
                // Horsey jump, so no legal captures or moves here.
                .WithDescription("Surrounded by friendly pieces (should still move freely)")
        );

        Add(
            PieceTestCase
                .From("e5", horsey)
                .WithEnemyPieceAt("d7")
                .WithEnemyPieceAt("f7")
                .WithEnemyPieceAt("c6")
                .WithEnemyPieceAt("g6")
                .WithEnemyPieceAt("c4")
                .WithEnemyPieceAt("g4")
                .WithEnemyPieceAt("d3")
                .WithEnemyPieceAt("f3")
                // All captures
                .GoesTo("d7", captures: ["d7"])
                .GoesTo("f7", captures: ["f7"])
                .GoesTo("c6", captures: ["c6"])
                .GoesTo("g6", captures: ["g6"])
                .GoesTo("c4", captures: ["c4"])
                .GoesTo("g4", captures: ["g4"])
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("f3", captures: ["f3"])
                .WithDescription("Surrounded by enemy pieces (all moves are captures)")
        );

        Add(
            PieceTestCase
                .From("b2", horsey)
                .GoesTo("a4") // up 2, left 1
                .GoesTo("c4") // up 2, right 1
                .GoesTo("d3") // up 1, right 2
                .GoesTo("d1") // down 1, right 2
                .WithDescription("Near bottom edge: b2")
        );
    }
}
