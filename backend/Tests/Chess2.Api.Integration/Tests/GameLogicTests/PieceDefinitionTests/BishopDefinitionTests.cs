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
        var friend = PieceFactory.White();
        var enemy = PieceFactory.Black();

        Add(
            PieceTestCase
                .From("e5", bishop)
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
                .WithPieceAt("g7", friend) // blocks beyond f6
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
                .WithPieceAt("g3", enemy) // enemy can be captured, blocks beyond
                .WithPieceAt("h2", friend) // friendly beyond enemy
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
                .WithPieceAt("d6", friend)
                .WithPieceAt("f6", friend)
                .WithPieceAt("d4", friend)
                .WithPieceAt("f4", friend)
                .WithDescription("Bishop surrounded by friendly pieces on all diagonals (no moves)")
        );

        Add(
            PieceTestCase
                .From("e5", bishop)
                .WithPieceAt("d6", enemy)
                .WithPieceAt("f6", enemy)
                .WithPieceAt("d4", enemy)
                .WithPieceAt("f4", enemy)
                .GoesTo("d6", captures: ["d6"])
                .GoesTo("f6", captures: ["f6"])
                .GoesTo("d4", captures: ["d4"])
                .GoesTo("f4", captures: ["f4"])
                .WithDescription(
                    "Bishop surrounded by enemy pieces on all diagonals (can capture all"
                )
        );
    }
}
