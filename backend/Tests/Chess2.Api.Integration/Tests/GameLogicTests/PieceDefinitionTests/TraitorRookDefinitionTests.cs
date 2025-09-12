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

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithWhitePieceAt("d6")
                .WithWhitePieceAt("d5")
                .WithBlackPieceAt("d4")
                .WithBlackPieceAt("h5") // to capture
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
                .WithBlackPieceAt("d6")
                .WithBlackPieceAt("d5")
                .WithWhitePieceAt("d4")
                .WithWhitePieceAt("h5") // to capture
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
                .WithWhitePieceAt("d4")
                .WithWhitePieceAt("d5")
                .WithWhitePieceAt("d6")
                .WithBlackPieceAt("f4")
                .WithBlackPieceAt("f5")
                .WithMovingPlayer(GameColor.Black)
                .WithDescription("White majority, black player moves, can't move")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithBlackPieceAt("d4")
                .WithBlackPieceAt("d5")
                .WithBlackPieceAt("d6")
                .WithWhitePieceAt("f4")
                .WithWhitePieceAt("f5")
                .WithMovingPlayer(GameColor.White)
                .WithDescription("Black majority, white player moves, can't move")
        );

        Add(
            PieceTestCase
                .From("e5", traitorRook)
                .WithWhitePieceAt("d4")
                .WithBlackPieceAt("d5")
                .WithWhitePieceAt("d6")
                .WithBlackPieceAt("f4")
                .WithWhitePieceAt("f5")
                .WithBlackPieceAt("f6")
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
                .WithWhitePieceAt("d5")
                .WithWhitePieceAt("f5")
                .WithWhitePieceAt("e4")
                .WithWhitePieceAt("e6")
                .WithMovingPlayer(GameColor.White)
                .WithDescription("Surrounded by friendly pieces, can't move")
        );

        Add(
            PieceTestCase
                .From("a1", traitorRook)
                .WithWhitePieceAt("a2")
                .WithBlackPieceAt("b1")
                .WithBlackPieceAt("b2")
                .WithMovingPlayer(GameColor.Black)
                .GoesTo("a2", captures: ["a2"])
                .WithDescription("Corner case a1, black majority, black moves")
        );
    }
}
