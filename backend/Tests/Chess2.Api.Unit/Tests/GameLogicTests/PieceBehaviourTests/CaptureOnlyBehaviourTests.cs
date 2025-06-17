using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceBehaviours;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;

public class CaptureOnlyBehaviourTests : MovementBasedPieceBehaviourTestBase
{
    [Fact]
    public void Evaluate_returns_only_moves_to_occupied_squares()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(new("b2"), PieceFactory.Black());
        board.PlacePiece(new("d4"), PieceFactory.White());

        var behaviour = new CaptureOnlyBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = new[]
        {
            new Move(Origin, new("b2"), piece, CapturedSquares: [new("b2")]),
            new Move(Origin, new("d4"), piece, CapturedSquares: [new("d4")]),
        };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_if_all_targets_are_empty()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        var behaviour = new CaptureOnlyBehaviour(MockMovement);
        var result = behaviour.Evaluate(board, Origin, piece);

        result.Should().BeEmpty();
    }
}
