using Chess2.Api.GameLogic;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameLogicTests.PieceDefinitionTests;

public abstract class PieceDefinitionTestBase : BaseIntegrationTest
{
    private readonly ILegalMoveCalculator _legalMoveCalculator;

    public PieceDefinitionTestBase(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _legalMoveCalculator = Scope.ServiceProvider.GetRequiredService<ILegalMoveCalculator>();
    }

    protected void TestMoves(PieceTestCase testCase)
    {
        var board = new ChessBoard();
        board.PlacePiece(testCase.Origin, testCase.Piece);

        foreach (var (point, piece) in testCase.BlockedBy)
            board.PlacePiece(point, piece);

        foreach (var priorMove in testCase.PriorMoves)
            board.PlayMove(priorMove);

        var result = _legalMoveCalculator
            .CalculateLegalMoves(board, testCase.Origin, testCase.MovingPlayer)
            .ToList();

        result.Should().BeEquivalentTo(testCase.ExpectedMoves);
    }
}
