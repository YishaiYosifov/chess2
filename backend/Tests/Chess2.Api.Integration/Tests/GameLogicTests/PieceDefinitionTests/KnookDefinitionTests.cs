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
                .GoesTo("b3") // up 2, right 1
                .GoesTo("c2") // up 1, right 2
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
                .GoesTo("b7") // up 2, right 1
                .GoesTo("c6") // up 1, right 2
                .GoesTo("c4") // down 1, right 2
                .GoesTo("b3") // down 2, right 1
                .WithDescription("Edge case: knook at a5")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithBlocker("e7", friend) // blocks beyond e6
                .WithBlocker("h5", friend) // blocks beyond g5
                .WithBlocker("f7", friend) // blocks horsey part
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
                .GoesTo("g6") // up 1, right 2
                .GoesTo("g4") // down 1, right 2
                .GoesTo("f3") // down 2, right 1
                .GoesTo("d3") // down 2, left 1
                .GoesTo("c4") // down 1, left 2
                .GoesTo("c6") // up 1, left 2
                .GoesTo("d7") // up 2, left 1
                .WithDescription("Blocked by friendly")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithBlocker("e6", friend)
                .WithBlocker("e4", friend)
                .WithBlocker("d5", friend)
                .WithBlocker("f5", friend)
                .WithBlocker("f5", friend)
                .WithBlocker("d7", friend)
                .WithBlocker("f7", friend)
                .WithBlocker("c6", friend)
                .WithBlocker("g6", friend)
                .WithBlocker("c4", friend)
                .WithBlocker("g4", friend)
                .WithBlocker("d3", friend)
                .WithBlocker("f3", friend)
                .WithDescription("Surrounded by friendly pieces in all directions")
        );

        Add(
            PieceTestCase
                .From("e5", knook)
                .WithBlocker("e6", enemy)
                .WithBlocker("e4", enemy)
                .WithBlocker("d5", enemy)
                .WithBlocker("f5", enemy)
                .WithBlocker("d7", enemy)
                .WithBlocker("f7", enemy)
                .WithBlocker("c6", enemy)
                .WithBlocker("g6", enemy)
                .WithBlocker("c4", enemy)
                .WithBlocker("g4", enemy)
                .WithBlocker("d3", enemy)
                .WithBlocker("f3", enemy)
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
