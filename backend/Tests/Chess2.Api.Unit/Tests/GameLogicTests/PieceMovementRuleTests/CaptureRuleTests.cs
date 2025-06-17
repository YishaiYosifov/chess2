using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class CaptureRuleTests : MovementBasedPieceRulesTestBase
{
    [Fact]
    public void Evaluate_returns_normal_moves_if_all_targets_are_empty()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        var behaviour = new CaptureRule(MockMovement);

        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        result.Should().BeEquivalentTo(Destinations.Select(to => new Move(Origin, to, piece)));
    }

    [Fact]
    public void Evaluate_includes_captures_when_enemy_is_on_target()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        var enemy = PieceFactory.Black();
        board.PlacePiece(new("b2"), enemy); // capture target

        var behaviour = new CaptureRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new Move[]
        {
            new(Origin, new("b2"), piece, CapturedSquares: [new("b2")]),
            new(Origin, new("c3"), piece),
            new(Origin, new("d4"), piece),
        };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_ignores_moves_to_friendly_pieces()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new("c3"), PieceFactory.White()); // friendly block

        var behaviour = new CaptureRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new Move[] { new(Origin, new("b2"), piece), new(Origin, new("d4"), piece) };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_handles_mixed_targets_correctly()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new("b2"), PieceFactory.Black()); // enemy
        board.PlacePiece(new("c3"), PieceFactory.White()); // ally

        var behaviour = new CaptureRule(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new Move[]
        {
            new(Origin, new("b2"), piece, CapturedSquares: [new("b2")]),
            new(Origin, new("d4"), piece),
        };

        result.Should().BeEquivalentTo(expected);
    }
}
