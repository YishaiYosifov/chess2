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

public class LegalMoveCalculatorTests : BaseIntegrationTest
{
    private readonly ILogger<LegalMoveCalculator> _loggerMock = Substitute.For<
        ILogger<LegalMoveCalculator>
    >();
    private readonly ILegalMoveCalculator _calculator;

    public LegalMoveCalculatorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _calculator = Scope.ServiceProvider.GetRequiredService<ILegalMoveCalculator>();
    }

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
        var board = new ChessBoard();
        board.PlacePiece(new AlgebraicPoint("a1"), PieceFactory.White(PieceType.Pawn));
        board.PlacePiece(new AlgebraicPoint("a3"), PieceFactory.Black(PieceType.King));

        var moves = _calculator.CalculateAllLegalMoves(board).ToList();

        moves.Should().HaveCount(6);

        var pawnMoves = moves.Where(x => x.Piece.Type == PieceType.Pawn);
        pawnMoves
            .Should()
            .ContainSingle()
            .Which.Should()
            .Satisfy<Move>(move =>
            {
                move.From.Should().BeEquivalentTo(new AlgebraicPoint("a1"));
                move.To.Should().BeEquivalentTo(new AlgebraicPoint("a2"));
            });
    }

    [Fact]
    public void CalculateAllLegalMoves_only_returns_the_moves_for_the_right_color()
    {
        var board = new ChessBoard();
        board.PlacePiece(new AlgebraicPoint("a1"), PieceFactory.White(PieceType.Pawn));
        board.PlacePiece(new AlgebraicPoint("a3"), PieceFactory.Black(PieceType.King));

        var moves = _calculator.CalculateAllLegalMoves(board, GameColor.White);

        moves
            .Should()
            .ContainSingle()
            .Which.Should()
            .Satisfy<Move>(move =>
            {
                move.From.Should().BeEquivalentTo(new AlgebraicPoint("a1"));
                move.To.Should().BeEquivalentTo(new AlgebraicPoint("a2"));
            });
    }
}
