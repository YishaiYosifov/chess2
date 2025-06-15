using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceBehaviours;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;

public class NoCaptureBehaviourTests : MovementBasedPieceBehaviourTestBase
{
    [Fact]
    public void Evaluate_returns_only_moves_to_empty_squares()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);
        board.PlacePiece(new(1, 1), PieceFactory.Black());
        board.PlacePiece(new(3, 3), PieceFactory.White());

        var behaviour = new NoCaptureBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();
        var expected = new[] { new Move(Origin, new(2, 2), piece) };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_if_all_targets_are_occupied()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);
        board.PlacePiece(new(1, 1), PieceFactory.Black());
        board.PlacePiece(new(2, 2), PieceFactory.White());
        board.PlacePiece(new(3, 3), PieceFactory.Black());

        var behaviour = new NoCaptureBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece);

        result.Should().BeEmpty();
    }
}
