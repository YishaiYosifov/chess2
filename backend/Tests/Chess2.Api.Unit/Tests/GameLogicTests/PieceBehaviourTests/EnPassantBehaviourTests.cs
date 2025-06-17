using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceBehaviours;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceBehaviourTests;

public class EnPassantBehaviourTests
{
    [Theory]
    [ClassData(typeof(EnPassantBehaviourTestData))]
    public void Evaluate_allows_en_passant_move_when_conditions_are_met(
        AlgebraicPoint origin,
        AlgebraicPoint destination,
        AlgebraicPoint enemyOrigin,
        AlgebraicPoint enemyDestination,
        Offset direction,
        GameColor capturingColor
    )
    {
        var board = new ChessBoard();
        var piece = new Piece(PieceType.Pawn, capturingColor);
        var enemyPiece = new Piece(PieceType.Pawn, capturingColor.Invert());

        board.PlacePiece(origin, piece);
        board.PlacePiece(enemyOrigin, enemyPiece);
        board.PlayMove(new Move(enemyOrigin, enemyDestination, enemyPiece));

        var behaviour = new EnPassantBehaviour(direction);

        var result = behaviour.Evaluate(board, origin, piece).ToList();

        var expected = new Move(
            From: origin,
            To: destination,
            Piece: piece,
            CapturedSquares: [enemyDestination]
        );

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [ClassData(typeof(InvalidEnPassantTestData))]
    public void Evaluate_disallows_en_passant_when_conditions_are_not_met(
        AlgebraicPoint origin,
        AlgebraicPoint enemyOrigin,
        AlgebraicPoint enemyDestination,
        Offset direction
    )
    {
        var board = new ChessBoard();
        var piece = PieceFactory.White(PieceType.Pawn);
        var enemy = PieceFactory.Black(PieceType.Pawn);

        board.PlacePiece(origin, piece);
        board.PlacePiece(enemyOrigin, enemy);
        board.PlayMove(new Move(enemyOrigin, enemyDestination, enemy));

        var behaviour = new EnPassantBehaviour(direction);
        var result = behaviour.Evaluate(board, origin, piece).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_no_last_move_exists()
    {
        var board = new ChessBoard();
        var pawn = PieceFactory.White(PieceType.Pawn);
        var origin = new AlgebraicPoint("e5");

        board.PlacePiece(origin, pawn);

        var behaviour = new EnPassantBehaviour(direction: new(X: -1, Y: 1));
        var result = behaviour.Evaluate(board, origin, pawn).ToList();

        result.Should().BeEmpty();
    }
}

public class EnPassantBehaviourTestData
    : TheoryData<
        AlgebraicPoint, // friendly pawn origin
        AlgebraicPoint, // destination after en passant
        AlgebraicPoint, // enemy pawn origin
        AlgebraicPoint, // enemy pawn destination after moving
        Offset, // direction of the en passant capture
        GameColor // color of the capturing pawn
    >
{
    public EnPassantBehaviourTestData()
    {
        // white capturing black
        // 2 steps
        Add(new("e7"), new("f8"), new("f9"), new("f7"), new(1, 1), GameColor.White);
        Add(new("e7"), new("d8"), new("d9"), new("d7"), new(-1, 1), GameColor.White);

        // 3 steps
        Add(new("e6"), new("f7"), new("f9"), new("f6"), new(1, 1), GameColor.White);
        Add(new("e6"), new("d7"), new("d9"), new("d6"), new(-1, 1), GameColor.White);

        // 4 steps
        Add(new("e5"), new("f6"), new("f9"), new("f5"), new(1, 1), GameColor.White);
        Add(new("e5"), new("d6"), new("d9"), new("d5"), new(-1, 1), GameColor.White);

        // black capturing white
        // 2 steps
        Add(new("e4"), new("f3"), new("f2"), new("f4"), new(1, -1), GameColor.Black);
        Add(new("e4"), new("d3"), new("d2"), new("d4"), new(-1, -1), GameColor.Black);

        // 3 steps
        Add(new("e5"), new("f4"), new("f2"), new("f5"), new(1, -1), GameColor.Black);
        Add(new("e5"), new("d4"), new("d2"), new("d5"), new(-1, -1), GameColor.Black);

        // 4 steps
        Add(new("e6"), new("f5"), new("f2"), new("f6"), new(1, -1), GameColor.Black);
        Add(new("e6"), new("d5"), new("d2"), new("d6"), new(-1, -1), GameColor.Black);

        // edge case: enemy pawn moved 5 squares, and we are capturing it from the 3rd square
        Add(new("e7"), new("d8"), new("d9"), new("d5"), new(-1, 1), GameColor.White);
    }
}

public class InvalidEnPassantTestData
    : TheoryData<
        AlgebraicPoint, // friendly pawn origin
        AlgebraicPoint, // enemy pawn origin
        AlgebraicPoint, // enemy pawn destination after moving
        Offset // direction of the en passant capture
    >
{
    public InvalidEnPassantTestData()
    {
        // enemy pawn only moved 1 square
        Add(new("e5"), new("d6"), new("d5"), new(-1, 1));

        // this would be a regular capture, so EnPassantBehaviour shouldn't pick up on it
        Add(new("e5"), new("d9"), new("d6"), new(-1, 1));

        // enemy pawn ends up on wrong file
        Add(new("e5"), new("c9"), new("c5"), new(-1, 1));

        // pawn trying to capture where there's no one to en passant
        Add(new("e5"), new("h2"), new("h4"), new(1, 1));
    }
}
