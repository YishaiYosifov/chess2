using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class EnPassantRuleTests
{
    private readonly Offset _nullChain = new();

    [Theory]
    [ClassData(typeof(EnPassantRuleTestData))]
    public void Evaluate_allows_en_passant_move_when_conditions_are_met(
        AlgebraicPoint origin,
        AlgebraicPoint destination,
        AlgebraicPoint enemyOrigin,
        AlgebraicPoint enemyDestination,
        Offset direction,
        GameColor capturingColor
    )
    {
        ChessBoard board = new();
        Piece piece = new(PieceType.Pawn, capturingColor);
        Piece enemyPiece = new(PieceType.Pawn, capturingColor.Invert());

        board.PlacePiece(origin, piece);
        board.PlacePiece(enemyOrigin, enemyPiece);
        board.PlayMove(new Move(enemyOrigin, enemyDestination, enemyPiece));

        EnPassantRule behaviour = new(direction, _nullChain);

        var result = behaviour.Evaluate(board, origin, piece).ToList();

        Move expected = new(
            from: origin,
            to: destination,
            piece: piece,
            capturedSquares: [enemyDestination],
            forcedPriority: ForcedMovePriority.EnPassant
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
        ChessBoard board = new();
        var piece = PieceFactory.White(PieceType.Pawn);
        var enemy = PieceFactory.Black(PieceType.Pawn);

        board.PlacePiece(origin, piece);
        board.PlacePiece(enemyOrigin, enemy);
        board.PlayMove(new Move(enemyOrigin, enemyDestination, enemy));

        var behaviour = new EnPassantRule(direction, _nullChain);
        var result = behaviour.Evaluate(board, origin, piece).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_returns_nothing_if_no_last_move_exists()
    {
        ChessBoard board = new();
        var pawn = PieceFactory.White(PieceType.Pawn);
        AlgebraicPoint origin = new("e5");

        board.PlacePiece(origin, pawn);

        EnPassantRule behaviour = new(direction: new(X: -1, Y: 1), _nullChain);
        var result = behaviour.Evaluate(board, origin, pawn).ToList();

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PieceType.UnderagePawn, true)]
    [InlineData(PieceType.Pawn, true)]
    [InlineData(PieceType.Rook, false)]
    public void Evaluate_only_allows_en_passant_for_the_correct_piece_type(
        PieceType enemyType,
        bool shouldWork
    )
    {
        ChessBoard board = new();
        var pawn = PieceFactory.White(PieceType.Pawn);
        var enemy = PieceFactory.Black(enemyType);

        AlgebraicPoint origin = new("e6");
        AlgebraicPoint enemyOrigin = new("d9");
        AlgebraicPoint enemyDestination = new("d6");

        board.PlacePiece(origin, pawn);
        board.PlacePiece(enemyOrigin, enemy);
        board.PlayMove(new Move(enemyOrigin, enemyDestination, enemy));
        EnPassantRule behaviour = new(direction: new(X: -1, Y: 1), _nullChain);

        var result = behaviour.Evaluate(board, origin, pawn).ToList();

        if (shouldWork)
            result.Should().NotBeEmpty();
        else
            result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_allows_en_passant_chain_capture_with_multiple_captures()
    {
        ChessBoard board = new();

        var whitePawn = PieceFactory.White(PieceType.Pawn);
        var blackPawn1 = PieceFactory.Black(PieceType.Pawn);
        var blackEnemy2 = PieceFactory.Black();
        var blackEnemy3 = PieceFactory.Black();
        var friendlyOnChain = PieceFactory.White();

        AlgebraicPoint origin = new("e5");
        AlgebraicPoint blackPawn1Start = new("d7");
        AlgebraicPoint blackPawn1End = new("d5");
        AlgebraicPoint blackEnemy2Pos = new("c6");
        AlgebraicPoint blackEnemy3Pos = new("b7");
        AlgebraicPoint whiteFriendlyOnChainPos = new("a8");

        board.PlacePiece(origin, whitePawn);
        board.PlacePiece(blackPawn1Start, blackPawn1);
        board.PlacePiece(blackEnemy2Pos, blackEnemy2);
        board.PlacePiece(blackEnemy3Pos, blackEnemy3);
        board.PlacePiece(whiteFriendlyOnChainPos, friendlyOnChain);

        board.PlayMove(new Move(blackPawn1Start, blackPawn1End, blackPawn1));

        EnPassantRule behaviour = new(new Offset(-1, 1), new Offset(0, -1));

        var moves = behaviour.Evaluate(board, origin, whitePawn).ToList();

        moves.Should().HaveCount(3);

        List<List<AlgebraicPoint>> expectedCapturesList =
        [
            [blackPawn1End],
            [blackPawn1End, blackEnemy2Pos],
            [blackPawn1End, blackEnemy2Pos, blackEnemy3Pos],
        ];

        for (int i = 0; i < moves.Count; i++)
        {
            moves[i].From.Should().Be(origin);
            moves[i].CapturedSquares.Should().BeEquivalentTo(expectedCapturesList[i]);
            moves[i].ForcedPriority.Should().Be(ForcedMovePriority.EnPassant);
        }

        moves[0].To.Should().Be(new AlgebraicPoint("d6"));
        moves[1].To.Should().Be(new AlgebraicPoint("c7"));
        moves[2].To.Should().Be(new AlgebraicPoint("b8"));
    }

    [Fact]
    public void Evaluate_stops_chain_capture_at_board_boundaries()
    {
        ChessBoard board = new();

        var whitePawn = PieceFactory.White(PieceType.Pawn);
        var blackPawn1 = PieceFactory.Black(PieceType.Pawn);

        // near left edge of board
        AlgebraicPoint origin = new("b7");
        AlgebraicPoint blackPawn1Start = new("a9");
        AlgebraicPoint blackPawn1End = new("a7");

        board.PlacePiece(origin, whitePawn);
        board.PlacePiece(blackPawn1Start, blackPawn1);

        board.PlayMove(new Move(blackPawn1Start, blackPawn1End, blackPawn1));

        EnPassantRule behaviour = new(new Offset(-1, 1), new Offset(0, -1));

        var moves = behaviour.Evaluate(board, origin, whitePawn).ToList();

        // Should only have the initial en passant move, chain capture stops at boundary
        moves.Should().HaveCount(1);
        moves[0].To.Should().Be(new AlgebraicPoint("a8"));
    }

    [Fact]
    public void Evaluate_stops_chain_capture_when_piece_blocks_the_chain()
    {
        ChessBoard board = new();

        var whitePawn = PieceFactory.White(PieceType.Pawn);
        var blackPawn1 = PieceFactory.Black(PieceType.Pawn);
        var blackEnemy1 = PieceFactory.Black();
        var blackBlocker = PieceFactory.Black();

        AlgebraicPoint origin = new("e5");
        AlgebraicPoint blackPawn1Start = new("d7");
        AlgebraicPoint blackPawn1End = new("d5");
        AlgebraicPoint blackEnemy1Pos = new("c6");
        AlgebraicPoint blackBlockerPos = new("c7");

        board.PlacePiece(origin, whitePawn);
        board.PlacePiece(blackPawn1Start, blackPawn1);
        board.PlacePiece(blackEnemy1Pos, blackEnemy1);
        board.PlacePiece(blackBlockerPos, blackBlocker);

        board.PlayMove(new Move(blackPawn1Start, blackPawn1End, blackPawn1));

        var behaviour = new EnPassantRule(new Offset(-1, 1), new Offset(0, -1));

        var moves = behaviour.Evaluate(board, origin, whitePawn).ToList();

        moves.Should().HaveCount(1);
        moves[0].To.Should().Be(new AlgebraicPoint("d6"));
        moves[0].CapturedSquares.Should().BeEquivalentTo([blackPawn1End]);
    }
}

public class EnPassantRuleTestData
    : TheoryData<
        AlgebraicPoint, // friendly pawn origin
        AlgebraicPoint, // destination after en passant
        AlgebraicPoint, // enemy pawn origin
        AlgebraicPoint, // enemy pawn destination after moving
        Offset, // direction of the en passant capture
        GameColor // color of the capturing pawn
    >
{
    public EnPassantRuleTestData()
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

        // this would be a regular capture, so it shouldn't pick up on it
        Add(new("e5"), new("d9"), new("d6"), new(-1, 1));

        // enemy pawn ends up on wrong file
        Add(new("e5"), new("c9"), new("c5"), new(-1, 1));

        // pawn trying to capture where there's no one to en passant
        Add(new("e5"), new("h2"), new("h4"), new(1, 1));
    }
}
