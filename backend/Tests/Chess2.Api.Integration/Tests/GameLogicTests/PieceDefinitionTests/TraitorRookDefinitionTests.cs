using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class TraitorRookDefinitionTests(Chess2WebApplicationFactory factory)
    : PieceDefinitionTestBase(factory)
{
    [Theory]
    [ClassData(typeof(TraitorRookDefinitionTestData))]
    public void TraitorRookDefinition_evaluates_expected_positions(PieceTestCase testCase) =>
        TestMoves(testCase);
}

public class TraitorRookDefinitionTestData : TheoryData<PieceTestCase>
{
    public TraitorRookDefinitionTestData()
    {
        var traitorRook = PieceFactory.Neutral(PieceType.TraitorRook);
        var white = PieceFactory.White();
        var black = PieceFactory.Black();

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithPieceAt("d6", white)
                .WithPieceAt("d5", white)
                .WithPieceAt("d4", black)
                .WithPieceAt("h5", black) // to capture
                .WithMovingPlayer(GameColor.White)
                // vertical moves
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                .GoesTo("e4")
                .GoesTo("e3")
                .GoesTo("e2")
                .GoesTo("e1")
                // horizontal moves
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5", captures: ["h5"])
                .WithDescription("White majority, white player moves, can capture black")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithPieceAt("d6", black)
                .WithPieceAt("d5", black)
                .WithPieceAt("d4", white)
                .WithPieceAt("h5", white) // to capture
                .WithMovingPlayer(GameColor.Black)
                // vertical moves
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                .GoesTo("e4")
                .GoesTo("e3")
                .GoesTo("e2")
                .GoesTo("e1")
                // horizontal moves
                .GoesTo("f5")
                .GoesTo("g5")
                .GoesTo("h5", captures: ["h5"])
                .WithDescription("Black majority, black player moves, can capture white")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithPieceAt("d4", white)
                .WithPieceAt("d5", white)
                .WithPieceAt("d6", white)
                .WithPieceAt("f4", black)
                .WithPieceAt("f5", black)
                .WithMovingPlayer(GameColor.Black)
                .WithDescription("White majority, black player moves, can't move")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithPieceAt("d4", black)
                .WithPieceAt("d5", black)
                .WithPieceAt("d6", black)
                .WithPieceAt("f4", white)
                .WithPieceAt("f5", white)
                .WithMovingPlayer(GameColor.White)
                .WithDescription("Black majority, white player moves, can't move")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithPieceAt("d4", white)
                .WithPieceAt("d5", black)
                .WithPieceAt("d6", white)
                .WithPieceAt("f4", black)
                .WithPieceAt("f5", white)
                .WithPieceAt("f6", black)
                .WithMovingPlayer(GameColor.White)
                // vertical moves
                .GoesTo("e6")
                .GoesTo("e7")
                .GoesTo("e8")
                .GoesTo("e9")
                .GoesTo("e10")
                .GoesTo("e4")
                .GoesTo("e3")
                .GoesTo("e2")
                .GoesTo("e1")
                .WithDescription("Tie with adjacent pieces, can move but can't capture")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithMovingPlayer(GameColor.White)
                .WithDescription("No adjacent pieces, can't move")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithPieceAt("d5", white)
                .WithPieceAt("f5", white)
                .WithPieceAt("e4", white)
                .WithPieceAt("e6", white)
                .WithMovingPlayer(GameColor.White)
                .WithDescription("Surrounded by friendly pieces, can't move")
        );

        Add(
            PieceTestCase
                .From("a1", traitorRook)
                .WithPieceAt("a2", white)
                .WithPieceAt("b1", black)
                .WithPieceAt("b2", black)
                .WithMovingPlayer(GameColor.Black)
                .GoesTo("a2", captures: ["a2"])
                .WithDescription("Corner case a1, black majority, black moves")
        );
    }
}
