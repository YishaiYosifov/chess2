using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class CheckerJumpRuleTests
{
    [Fact]
    public void Evaluate_returns_no_moves_when_there_are_no_pieces_to_capture()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        AlgebraicPoint origin = new("e6");
        board.PlacePiece(origin, piece);

        CheckerJumpRule rule = new(new Offset(1, 1));

        var moves = rule.Evaluate(board, origin, piece).ToList();

        moves.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_a_single_capture_move_when_one_enemy_piece_is_in_range()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        Piece enemy = PieceFactory.Black();

        AlgebraicPoint origin = new("e6");
        AlgebraicPoint enemyPosition = new("d7");
        AlgebraicPoint landing = new("c8");

        board.PlacePiece(origin, piece);
        board.PlacePiece(enemyPosition, enemy);

        CheckerJumpRule rule = new(new Offset(-1, 1));

        var moves = rule.Evaluate(board, origin, piece).ToList();

        Move expected = new(
            origin,
            landing,
            piece,
            captures: [new MoveCapture(enemyPosition, board)]
        );

        moves.Should().BeEquivalentTo([expected]);
    }

    [Fact]
    public void Evaluate_allows_jumping_over_friendly_piece_without_capturing_it()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        Piece friendly = PieceFactory.White();

        AlgebraicPoint origin = new("e6");
        AlgebraicPoint friendlyPosition = new("d7");
        AlgebraicPoint landing = new("c8");

        board.PlacePiece(origin, piece);
        board.PlacePiece(friendlyPosition, friendly);

        CheckerJumpRule rule = new(new Offset(-1, 1));

        var moves = rule.Evaluate(board, origin, piece).ToList();

        Move expected = new(origin, landing, piece);

        moves.Should().BeEquivalentTo([expected]);
    }

    [Fact]
    public void Evaluate_returns_multiple_immediate_jumps_as_separate_moves()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        Piece enemyLeft = PieceFactory.Black();
        Piece enemyRight = PieceFactory.Black();

        AlgebraicPoint origin = new("e6");
        AlgebraicPoint enemyLeftPosition = new("d7");
        AlgebraicPoint landingLeft = new("c8");
        AlgebraicPoint enemyRightPosition = new("f7");
        AlgebraicPoint landingRight = new("g8");

        board.PlacePiece(origin, piece);
        board.PlacePiece(enemyLeftPosition, enemyLeft);
        board.PlacePiece(enemyRightPosition, enemyRight);

        CheckerJumpRule rule = new(new Offset(-1, 1), new Offset(1, 1));

        var moves = rule.Evaluate(board, origin, piece).ToList();

        Move leftMove = new(
            origin,
            landingLeft,
            piece,
            captures: [new MoveCapture(enemyLeftPosition, board)]
        );

        Move rightMove = new(
            origin,
            landingRight,
            piece,
            captures: [new MoveCapture(enemyRightPosition, board)]
        );

        moves.Should().BeEquivalentTo([leftMove, rightMove]);
    }

    [Fact]
    public void Evaluate_returns_all_partial_and_full_moves_in_branching_chain()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        Piece firstEnemy = PieceFactory.Black();
        Piece branchEnemy1 = PieceFactory.Black();
        Piece branchEnemy2 = PieceFactory.Black();

        AlgebraicPoint origin = new("c3");
        AlgebraicPoint firstEnemyPosition = new("d4");
        AlgebraicPoint firstLanding = new("e5");
        AlgebraicPoint branchEnemy1Position = new("f6");
        AlgebraicPoint landingRight = new("g7");
        AlgebraicPoint branchEnemy2Position = new("d6");
        AlgebraicPoint landingLeft = new("c7");

        board.PlacePiece(origin, piece);
        board.PlacePiece(firstEnemyPosition, firstEnemy);
        board.PlacePiece(branchEnemy1Position, branchEnemy1);
        board.PlacePiece(branchEnemy2Position, branchEnemy2);

        CheckerJumpRule rule = new(new Offset(-1, 1), new Offset(1, 1));

        var moves = rule.Evaluate(board, origin, piece).ToList();

        Move moveAfterFirstJump = new(
            origin,
            firstLanding,
            piece,
            captures: [new MoveCapture(firstEnemyPosition, board)]
        );

        Move moveRightBranch = new(
            origin,
            landingRight,
            piece,
            captures:
            [
                new MoveCapture(firstEnemyPosition, board),
                new MoveCapture(branchEnemy1Position, board),
            ],
            intermediateSquares: [firstLanding]
        );

        Move moveLeftBranch = new(
            origin,
            landingLeft,
            piece,
            captures:
            [
                new MoveCapture(firstEnemyPosition, board),
                new MoveCapture(branchEnemy2Position, board),
            ],
            intermediateSquares: [firstLanding]
        );

        moves.Should().BeEquivalentTo([moveAfterFirstJump, moveRightBranch, moveLeftBranch]);
    }

    [Fact]
    public void Evaluate_allows_mixed_chain_with_friendly_and_enemy_jumps_generating_multiple_moves()
    {
        ChessBoard board = new();
        Piece piece = PieceFactory.White();
        Piece friendly = PieceFactory.White();
        Piece enemy = PieceFactory.Black();

        AlgebraicPoint origin = new("a1");
        AlgebraicPoint friendlyPosition = new("b2");
        AlgebraicPoint firstLanding = new("c3");
        AlgebraicPoint enemyPosition = new("d4");
        AlgebraicPoint finalLanding = new("e5");

        board.PlacePiece(origin, piece);
        board.PlacePiece(friendlyPosition, friendly);
        board.PlacePiece(enemyPosition, enemy);

        CheckerJumpRule rule = new(new Offset(1, 1));

        var moves = rule.Evaluate(board, origin, piece).ToList();

        Move moveAfterFriendlyJump = new(origin, firstLanding, piece);

        Move moveAfterEnemyJump = new(
            origin,
            finalLanding,
            piece,
            captures: [new MoveCapture(enemyPosition, board)],
            intermediateSquares: [firstLanding]
        );

        moves.Should().BeEquivalentTo([moveAfterFriendlyJump, moveAfterEnemyJump]);
    }
}
