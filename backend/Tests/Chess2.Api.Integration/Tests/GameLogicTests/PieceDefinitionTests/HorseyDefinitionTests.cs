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
        var friend = PieceFactory.White();
        var enemy = PieceFactory.Black();

        // Open board from e5
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
        );

        // Corner case: knight at a1
        Add(
            PieceTestCase
                .From("a1", horsey)
                // only 2 valid moves
                .GoesTo("b3") // up 2, right 1
                .GoesTo("c2") // up 1, right 2
        );

        // Corner case: knight at j10
        Add(
            PieceTestCase
                .From("j10", horsey)
                // only 2 valid moves
                .GoesTo("h9") // down 1, left 2
                .GoesTo("i8") // down 2, left 1
        );

        // Edge case: knight at a5
        Add(
            PieceTestCase
                .From("a5", horsey)
                // 4 valid moves
                .GoesTo("b7") // up 2, right 1
                .GoesTo("c6") // up 1, right 2
                .GoesTo("c4") // down 1, right 2
                .GoesTo("b3") // down 2, right 1
        );

        // Surrounded by friendly pieces (should still move freely)
        Add(
            PieceTestCase
                .From("e5", horsey)
                .WithBlocker("d7", friend)
                .WithBlocker("f7", friend)
                .WithBlocker("c6", friend)
                .WithBlocker("g6", friend)
                .WithBlocker("c4", friend)
                .WithBlocker("g4", friend)
                .WithBlocker("d3", friend)
                .WithBlocker("f3", friend)
        // Knights jump, so no legal captures or moves here.
        );

        // Surrounded by enemy pieces (all moves are captures)
        Add(
            PieceTestCase
                .From("e5", horsey)
                .WithBlocker("d7", enemy)
                .WithBlocker("f7", enemy)
                .WithBlocker("c6", enemy)
                .WithBlocker("g6", enemy)
                .WithBlocker("c4", enemy)
                .WithBlocker("g4", enemy)
                .WithBlocker("d3", enemy)
                .WithBlocker("f3", enemy)
                // All captures
                .GoesTo("d7", captures: ["d7"])
                .GoesTo("f7", captures: ["f7"])
                .GoesTo("c6", captures: ["c6"])
                .GoesTo("g6", captures: ["g6"])
                .GoesTo("c4", captures: ["c4"])
                .GoesTo("g4", captures: ["g4"])
                .GoesTo("d3", captures: ["d3"])
                .GoesTo("f3", captures: ["f3"])
        );

        // Near bottom edge: b2
        Add(
            PieceTestCase
                .From("b2", horsey)
                // only 4 valid moves
                .GoesTo("a4") // up 2, left 1
                .GoesTo("c4") // up 2, right 1
                .GoesTo("d3") // up 1, right 2
                .GoesTo("d1") // down 1, right 2
        );
    }
}
