using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public class QueenDefinitionTests : BaseIntegrationTest
{
    private readonly ILegalMoveCalculator _legalMoveCalculator;

    public QueenDefinitionTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _legalMoveCalculator = Scope.ServiceProvider.GetRequiredService<ILegalMoveCalculator>();
    }

    [Theory]
    [ClassData(typeof(QueenDefinitionTestData))]
    public void QueenDefinition_evaluates_expected_positions(PieceTestCase testCase)
    {
        var queen = PieceFactory.White(PieceType.Queen);
        var board = new ChessBoard();
        board.PlacePiece(testCase.Origin, queen);

        foreach (var (point, piece) in testCase.BlockedBy ?? [])
            board.PlacePiece(point, piece);

        var result = _legalMoveCalculator.CalculateLegalMoves(board, testCase.Origin).ToList();

        result.Should().BeEquivalentTo(testCase.ExpectedMoves);
    }
}

public class QueenDefinitionTestData : TheoryData<PieceTestCase>
{
    public QueenDefinitionTestData()
    {
        var queen = PieceFactory.White(PieceType.Queen);
        var friend = PieceFactory.White();
        var enemy = PieceFactory.Black();

        // Open board from d4
        Add(
            PieceTestCase
                .From("d4", queen)
                // vertical up
                .GoesTo("d5")
                .GoesTo("d6")
                .GoesTo("d7")
                .GoesTo("d8")
                .GoesTo("d9")
                .GoesTo("d10")
                // vertical down
                .GoesTo("d3")
                .GoesTo("d2")
                .GoesTo("d1")
                // horizontal right
                .GoesTo("e4")
                .GoesTo("f4")
                .GoesTo("g4")
                .GoesTo("h4")
                .GoesTo("i4")
                .GoesTo("j4")
                // horizontal left
                .GoesTo("c4")
                .GoesTo("b4")
                .GoesTo("a4")
                // diagonal up-right
                .GoesTo("e5")
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
                // diagonal up-left
                .GoesTo("c5")
                .GoesTo("b6")
                .GoesTo("a7")
                // diagonal down-right
                .GoesTo("e3")
                .GoesTo("f2")
                .GoesTo("g1")
                // diagonal down-left
                .GoesTo("c3")
                .GoesTo("b2")
                .GoesTo("a1")
        );

        // Own piece blocks on f4, black pawn on b6 (can capture)
        Add(
            PieceTestCase
                .From("d4", queen)
                .WithBlocker("f4", friend)
                .WithBlocker("b6", enemy)
                .GoesTo("d5")
                .GoesTo("d6")
                .GoesTo("d7")
                .GoesTo("d8")
                .GoesTo("d9")
                .GoesTo("d10")
                // vertical down moves unblocked
                .GoesTo("d3")
                .GoesTo("d2")
                .GoesTo("d1")
                // horizontal right stops before f4, no move onto f4 because own piece
                .GoesTo("e4")
                // horizontal left fully free
                .GoesTo("c4")
                .GoesTo("b4")
                .GoesTo("a4")
                // diagonal up-right unblocked
                .GoesTo("e5")
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
                // diagonal up-left captures b6
                .GoesTo("c5")
                .GoesTo("b6", captures: ["b6"])
                // diagonal down-right unblocked
                .GoesTo("e3")
                .GoesTo("f2")
                .GoesTo("g1")
                // diagonal down-left unblocked
                .GoesTo("c3")
                .GoesTo("b2")
                .GoesTo("a1")
        );

        // From a corner
        Add(
            PieceTestCase
                .From("a1", queen)
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
                // diagonal up-right
                .GoesTo("b2")
                .GoesTo("c3")
                .GoesTo("d4")
                .GoesTo("e5")
                .GoesTo("f6")
                .GoesTo("g7")
                .GoesTo("h8")
                .GoesTo("i9")
                .GoesTo("j10")
        );
    }
}
