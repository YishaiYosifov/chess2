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
        ChessBoard board = new();
        var piece = PieceFactory.White();
        var pieceToCapture = PieceFactory.Black();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(Destinations[0], pieceToCapture); // enemy
        board.PlacePiece(Destinations[1], PieceFactory.White()); // friend

        CaptureOnlyRule behaviour = new(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        Move expectedMove = new(
            Origin,
            Destinations[0],
            piece,
            captures: [new MoveCapture(pieceToCapture, Destinations[0])]
        );
        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_returns_empty_if_all_targets_are_empty()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        CaptureOnlyRule behaviour = new(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece);

        result.Should().BeEmpty();
    }
}
