using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceDefinitions;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.GameLogicTests;

public class LegalMoveCalculatorTests(Chess2WebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    private readonly ILogger<LegalMoveCalculator> _loggerMock = Substitute.For<
        ILogger<LegalMoveCalculator>
    >();

    [Fact]
    public void Constructor_throws_on_duplicate_piecedefinitions()
    {
        var pieceType = PieceType.Pawn;

        var pieceDefMock1 = Substitute.For<IPieceDefinition>();
        pieceDefMock1.Type.Returns(pieceType);

        var pieceDefMock2 = Substitute.For<IPieceDefinition>();
        pieceDefMock2.Type.Returns(pieceType);

        var act = () => new LegalMoveCalculator(_loggerMock, [pieceDefMock1, pieceDefMock2]);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"Duplicate piece definition for {pieceType}");
    }

    [Fact]
    public void Constructor_throws_if_not_all_piece_types_are_defined()
    {
        // Provide fewer pieces than total enum values
        var act = () => new LegalMoveCalculator(_loggerMock, []);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Could not find definitions for all pieces");
    }

    [Fact]
    public void CalculateAllLegalMoves_returns_expected_moves_from_pieces()
    {
        var calculator = Scope.ServiceProvider.GetRequiredService<ILegalMoveCalculator>();

        var board = new ChessBoard();
        board.PlacePiece(new AlgebraicPoint("a1"), PieceFactory.White(PieceType.Pawn));
        board.PlacePiece(new AlgebraicPoint("a3"), PieceFactory.Black(PieceType.King));

        var moves = calculator.CalculateAllLegalMoves(board).ToList();

        moves.Should().HaveCount(6);

        var pawnMoves = moves.Where(x => x.Piece.Type == PieceType.Pawn);
        pawnMoves
            .Should()
            .ContainSingle()
            .Which.Should()
            .Satisfy<Move>(
                (Action<Move>)(
                    move =>
                    {
                        move.From.Should().BeEquivalentTo(new AlgebraicPoint("a1"));
                        move.To.Should().BeEquivalentTo(new AlgebraicPoint("a2"));
                    }
                )
            );
    }
}
