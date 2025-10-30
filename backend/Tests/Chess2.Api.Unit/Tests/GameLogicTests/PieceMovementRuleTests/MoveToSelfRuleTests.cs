using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class MoveToSelfRuleTests
{
    [Theory]
    [InlineData(GameColor.White)]
    [InlineData(GameColor.Black)]
    public void Evaluate_returns_move_to_same_square(GameColor color)
    {
        ChessBoard board = new();
        Piece piece = new PieceFaker(color).Generate();
        AlgebraicPoint origin = new("e4");

        board.PlacePiece(origin, piece);
        MoveToSelfRule rule = new();

        var moves = rule.Evaluate(board, origin, piece).ToList();

        Move expected = new(origin, origin, piece);
        moves.Should().BeEquivalentTo([expected]);
    }
}
