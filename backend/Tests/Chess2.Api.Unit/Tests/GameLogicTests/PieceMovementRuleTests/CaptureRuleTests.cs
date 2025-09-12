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

        CaptureRule behaviour = new(MovementMocks);

        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        result.Should().BeEquivalentTo(CreateMoves(piece, Destinations));
    }

    [Fact]
    public void Evaluate_includes_captures_when_enemy_is_on_target()
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        var enemy = PieceFactory.Black();
        board.PlacePiece(Destinations[0], enemy); // capture target

        CaptureRule behaviour = new(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        Move expectedCapture = new(
            Origin,
            Destinations[0],
            piece,
            captures: [new MoveCapture(enemy, Destinations[0])]
        );
        result.Should().BeEquivalentTo([expectedCapture, .. CreateMoves(piece, Destinations[1..])]);
    }

    [Fact]
    public void Evaluate_ignores_moves_to_friendly_pieces()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(Destinations[1], PieceFactory.White()); // friendly block

        CaptureRule behaviour = new(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        result.Should().BeEquivalentTo(CreateMoves(piece, [Destinations[0], .. Destinations[2..]]));
    }

    [Fact]
    public void Evaluate_handles_mixed_targets_correctly()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        var pieceToCapture = PieceFactory.Black();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(Destinations[0], pieceToCapture); // enemy
        board.PlacePiece(Destinations[1], PieceFactory.White()); // ally

        CaptureRule behaviour = new(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        Move expectedCapture = new(
            Origin,
            Destinations[0],
            piece,
            captures: [new MoveCapture(pieceToCapture, Destinations[0])]
        );

        result.Should().BeEquivalentTo([expectedCapture, .. CreateMoves(piece, Destinations[2..])]);
    }

    [Fact]
    public void Evaluate_allows_friendly_fire_only_for_specified_piece()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        var friendlyAllowed = PieceFactory.White(PieceType.Horsey);
        var friendlyBlocked = PieceFactory.White(PieceType.Rook);

        board.PlacePiece(Origin, piece);
        board.PlacePiece(Destinations[0], friendlyAllowed);
        board.PlacePiece(Destinations[1], friendlyBlocked);

        CaptureRule rule = CaptureRule.WithFriendlyFire(
            (board, piece) => piece.Type == friendlyAllowed.Type,
            MovementMocks
        );
        var result = rule.Evaluate(board, Origin, piece).ToList();

        Move expectedCapture = new(
            Origin,
            Destinations[0],
            piece,
            captures: [new MoveCapture(friendlyAllowed, Destinations[0])]
        );
        result.Should().BeEquivalentTo([expectedCapture, .. CreateMoves(piece, Destinations[2..])]);
    }

    [Fact]
    public void Evaluate_allows_neutral_capture_when_specified()
    {
        ChessBoard board = new();
        var piece = PieceFactory.Neutral();
        var capturePiece = PieceFactory.White();
        var noCapturePiece = PieceFactory.Black();

        board.PlacePiece(Origin, piece);
        board.PlacePiece(Destinations[0], noCapturePiece);
        board.PlacePiece(Destinations[1], capturePiece);

        CaptureRule rule = CaptureRule.WithNeutralCapture(
            (board, target) => target == capturePiece,
            MovementMocks
        );
        var result = rule.Evaluate(board, Origin, piece).ToList();

        Move expectedCapture = new(
            Origin,
            Destinations[1],
            piece,
            captures: [new MoveCapture(capturePiece, Destinations[1])]
        );
        result.Should().BeEquivalentTo([expectedCapture, .. CreateMoves(piece, Destinations[2..])]);
    }

    [Fact]
    public void Evaluate_allows_neutral_capture_by_default()
    {
        ChessBoard board = new();
        var piece = PieceFactory.Neutral();
        var capturePiece = PieceFactory.White();

        board.PlacePiece(Origin, piece);
        board.PlacePiece(Destinations[0], capturePiece);

        CaptureRule rule = new(MovementMocks);
        var result = rule.Evaluate(board, Origin, piece).ToList();

        Move expectedCapture = new(
            Origin,
            Destinations[0],
            piece,
            captures: [new MoveCapture(capturePiece, Destinations[0])]
        );
        result.Should().BeEquivalentTo([expectedCapture, .. CreateMoves(piece, Destinations[1..])]);
    }
}
