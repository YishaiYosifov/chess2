using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class NoCaptureRuleTests : MovementBasedPieceRulesTestBase
{
    [Fact]
    public void Evaluate_returns_only_moves_to_empty_squares()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new("b2"), PieceFactory.Black());
        board.PlacePiece(new("d4"), PieceFactory.White());

        var behaviour = new NoCaptureRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new[] { new Move(Origin, new("c3"), piece) };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_if_all_targets_are_occupied()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new("b2"), PieceFactory.Black());
        board.PlacePiece(new("c3"), PieceFactory.White());
        board.PlacePiece(new("d4"), PieceFactory.Black());

        var behaviour = new NoCaptureRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece);

        result.Should().BeEmpty();
    }
}
