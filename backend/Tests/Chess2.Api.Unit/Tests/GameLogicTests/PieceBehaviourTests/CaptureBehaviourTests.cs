using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceBehaviours;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;

public class CaptureBehaviourTests : MovementBasedPieceBehaviourTestBase
{
    [Fact]
    public void Evaluate_returns_normal_moves_if_all_targets_are_empty()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        var behaviour = new CaptureBehaviour(MockMovement);

        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        result.Should().BeEquivalentTo(Destinations.Select(to => new Move(Origin, to, piece)));
    }

    [Fact]
    public void Evaluate_includes_captures_when_enemy_is_on_target()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        // enemy piece at (1,1)
        var enemy = PieceFactory.Black();
        board.PlacePiece(new(1, 1), enemy);

        var behaviour = new CaptureBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new Move[]
        {
            new(Origin, new(1, 1), piece, CapturedSquares: [new(1, 1)]),
            new(Origin, new(2, 2), piece),
            new(Origin, new(3, 3), piece),
        };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_ignores_moves_to_friendly_pieces()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        // friendly piece at (2,2)
        board.PlacePiece(new(2, 2), PieceFactory.White());

        var behaviour = new CaptureBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new Move[] { new(Origin, new(1, 1), piece), new(Origin, new(3, 3), piece) };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_handles_mixed_targets_correctly()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new(1, 1), PieceFactory.Black()); // capture
        board.PlacePiece(new(2, 2), PieceFactory.White()); // skip

        var behaviour = new CaptureBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new Move[]
        {
            new(Origin, new(1, 1), piece, CapturedSquares: [new(1, 1)]),
            new(Origin, new(3, 3), piece),
        };

        result.Should().BeEquivalentTo(expected);
    }
}
