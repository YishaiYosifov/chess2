using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class CaptureOnlyRuleTests : MovementBasedPieceRulesTestBase
{
    [Fact]
    public void Evaluate_returns_only_moves_to_occupied_squares()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new("b2"), PieceFactory.Black()); // enemy
        board.PlacePiece(new("d4"), PieceFactory.White()); // friend

        var behaviour = new CaptureOnlyRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new[] { new Move(Origin, new("b2"), piece, capturedSquares: [new("b2")]) };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_if_all_targets_are_empty()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        var behaviour = new CaptureOnlyRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece);

        result.Should().BeEmpty();
    }
}
