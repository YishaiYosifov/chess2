using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
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
    public void Constructor_throws_if_not_all_piece_types_are_defined()
    {
        var act = () => new LegalMoveCalculator(_loggerMock, []);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Could not find definitions for all pieces");
    }

    [Fact]
    public void CalculateAllLegalMoves_only_returns_the_moves_for_the_right_color()
    {
        ChessBoard board = new();
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

    [Fact]
    public void CalculateLegalMoves_allows_moves_for_piece_with_neutral_color()
    {
        ChessBoard board = new();
        var neutralRook = PieceFactory.Neutral(PieceType.TraitorRook);
        board.PlacePiece(new AlgebraicPoint("d4"), neutralRook);

        // surround it with equal white and black pieces to trigger neutral behavior
        board.PlacePiece(new AlgebraicPoint("c3"), PieceFactory.White(PieceType.Pawn));
        board.PlacePiece(new AlgebraicPoint("e5"), PieceFactory.Black(PieceType.Pawn));

        var movesForWhite = _calculator.CalculateAllLegalMoves(board, GameColor.White).ToList();
        var movesForBlack = _calculator.CalculateAllLegalMoves(board, GameColor.Black).ToList();

        movesForWhite.Should().Contain(move => move.Piece.Type == PieceType.TraitorRook);
        movesForBlack.Should().Contain(move => move.Piece.Type == PieceType.TraitorRook);
    }
}
