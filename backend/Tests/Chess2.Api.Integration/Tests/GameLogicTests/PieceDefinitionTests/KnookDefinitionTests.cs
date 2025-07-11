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
        var friend = PieceFactory.White();
        var enemy = PieceFactory.Black();

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
                .WithPieceAt("e7", friend) // blocks beyond e6
                .WithPieceAt("h5", friend) // blocks beyond g5
                .WithPieceAt("f7", friend) // blocks horsey part
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
                .WithPieceAt("e6", friend)
                .WithPieceAt("e4", friend)
                .WithPieceAt("d5", friend)
                .WithPieceAt("f5", friend)
                .WithPieceAt("f5", friend)
                .WithPieceAt("d7", friend)
                .WithPieceAt("f7", friend)
                .WithPieceAt("c6", friend)
                .WithPieceAt("g6", friend)
                .WithPieceAt("c4", friend)
                .WithPieceAt("g4", friend)
                .WithPieceAt("d3", friend)
                .WithPieceAt("f3", friend)
                .WithDescription("Surrounded by friendly pieces in all directions")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithPieceAt("e6", enemy)
                .WithPieceAt("e4", enemy)
                .WithPieceAt("d5", enemy)
                .WithPieceAt("f5", enemy)
                .WithPieceAt("d7", enemy)
                .WithPieceAt("f7", enemy)
                .WithPieceAt("c6", enemy)
                .WithPieceAt("g6", enemy)
                .WithPieceAt("c4", enemy)
                .WithPieceAt("g4", enemy)
                .WithPieceAt("d3", enemy)
                .WithPieceAt("f3", enemy)
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
