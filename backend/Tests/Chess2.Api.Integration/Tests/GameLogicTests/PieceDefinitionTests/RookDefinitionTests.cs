using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class RookDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(RookDefinitionTestData))]
    public void RookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class RookDefinitionTestData : TheoryData<PieceTestCase>
{
    public RookDefinitionTestData()
    {
        var rook = PieceFactory.White(PieceType.Rook);
        var friend = PieceFactory.White();
        var enemy = PieceFactory.Black();

        Add(
            PieceTestCase
                .From("e5", rook)
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
                .WithDescription("Open board from e5")
        );

        Add(
            PieceTestCase
                .From("a1", rook)
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
                .WithDescription("Corner case: rook at a1")
        );

        Add(
            PieceTestCase
                .From("a5", rook)
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
                .WithDescription("Edge case: rook at a5")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithPieceAt("e7", friend) // blocks beyond e6
                .WithPieceAt("h5", friend) // blocks beyond g5
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
                .WithDescription("Blocked by friendly piece up and right")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithPieceAt("e3", enemy) // can capture
                .WithPieceAt("b5", enemy) // can capture
                // vertical up
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                // vertical down
                .GoesTo("e4")
                .GoesTo("e3", captures: ["e3"])
                // horizontal left
                .GoesTo("d5")
                .GoesTo("c5")
                .GoesTo("b5", captures: ["b5"])
                // horizontal right
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5")
                .GoesTo("i5")
                .GoesTo("j5")
                .WithDescription("Blocked by enemy piece down and left")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithPieceAt("e6", friend)
                .WithPieceAt("e4", friend)
                .WithPieceAt("d5", friend)
                .WithPieceAt("f5", friend)
                .WithDescription("Surrounded by friendly pieces in all directions")
        );

        Add(
            PieceTestCase
                .From("e5", rook)
                .WithPieceAt("e6", enemy)
                .WithPieceAt("e4", enemy)
                .WithPieceAt("d5", enemy)
                .WithPieceAt("f5", enemy)
                .GoesTo("e6", captures: ["e6"])
                .GoesTo("e4", captures: ["e4"])
                .GoesTo("d5", captures: ["d5"])
                .GoesTo("f5", captures: ["f5"])
                .WithDescription("Surrounded by enemy pieces in all directions")
        );
    }
}
