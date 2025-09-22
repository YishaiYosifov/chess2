using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class SanCalculatorTests
{
    private readonly SanCalculator _calculator = new(new PieceToLetter());

    [Fact]
    public void CalculateSan_doesnt_disambiguate_when_not_necessary()
    {
        Move move = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("Hc3");
    }

    [Fact]
    public void CalculateSan_disambiguates_by_file()
    {
        Move move1 = new(new("b1"), new("d2"), PieceFactory.White(PieceType.Horsey));
        Move move2 = new(new("f1"), new("d2"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move1, [move1, move2]);

        san.Should().Be("Hbd2");
    }

    [Fact]
    public void CalculateSan_disambiguates_by_rank()
    {
        Move move1 = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move move2 = new(new("b6"), new("c3"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move1, [move1, move2]);

        san.Should().Be("H1c3");
    }

    [Fact]
    public void CalculateSan_disambiguates_by_both_file_and_rank()
    {
        Move move1 = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move move2 = new(new("b4"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move move3 = new(new("e1"), new("c3"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move1, [move1, move2, move3]);

        san.Should().Be("Hb1c3");
    }

    [Fact]
    public void CalculateSan_doesnt_disambiguate_with_the_same_piece_type_on_different_destination()
    {
        Move move = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move otherMove = new(new("d2"), new("d4"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move, [move, otherMove]);

        san.Should().Be("Hc3");
    }

    [Fact]
    public void CalculateSan_doesnt_disambiguate_with_different_piece_type_to_same_destination()
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White(PieceType.Rook));
        Move otherMove = new(new("e7"), new("e4"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move, [move, otherMove]);

        san.Should().Be("Re4");
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    public void CalculateSan_doesnt_add_the_piece_letter_for_pawns(PieceType pawnType)
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White(pawnType));

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("e4");
    }

    [Fact]
    public void CalculateSan_includes_destination_when_it_is_not_a_capture()
    {
        Move move = new(
            new("d4"),
            new("e5"),
            PieceFactory.White(PieceType.Rook),
            captures:
            [
                new MoveCapture(PieceFactory.Black(), new("c4")),
                new MoveCapture(PieceFactory.Black(), new("d5")),
            ]
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("Re5xc4xd5");
    }

    [Fact]
    public void CalculateSan_adds_the_destination_to_the_start_when_multi_capturing()
    {
        Move move = new(
            new("d4"),
            new("d5"),
            PieceFactory.White(PieceType.Rook),
            captures:
            [
                new MoveCapture(PieceFactory.Black(), new("c4")),
                new MoveCapture(PieceFactory.Black(), new("d5")),
            ]
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("Rxd5xc4");
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    public void CalculateSan_handles_pawn_multi_capture_and_adds_file_letter(PieceType pawnType)
    {
        Move move = new(
            new("e5"),
            new("f6"),
            PieceFactory.White(pawnType),
            captures:
            [
                new MoveCapture(PieceFactory.Black(), new("f5")),
                new MoveCapture(PieceFactory.Black(), new("f6")),
            ]
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("exf6xf5");
    }

    [Fact]
    public void CalculateSan_handles_single_capture_when_destination_is_capture()
    {
        Move move = new(
            new("e5"),
            new("f6"),
            PieceFactory.White(PieceType.Pawn),
            captures: [new MoveCapture(PieceFactory.Black(), new("f6"))]
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("exf6");
    }

    [Theory]
    [InlineData(SpecialMoveType.KingsideCastle, "O-O")]
    [InlineData(SpecialMoveType.QueensideCastle, "O-O-O")]
    public void CalculateSan_uses_castle_notation(SpecialMoveType moveType, string expectedNotation)
    {
        Move move = new(
            new("a1"),
            new("a2"),
            PieceFactory.White(PieceType.King),
            specialMoveType: moveType
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be(expectedNotation);
    }

    [Fact]
    public void CalculateSan_adds_captures_for_castling()
    {
        Move move = new(
            new("a1"),
            new("a2"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.KingsideCastle,
            captures: [new MoveCapture(PieceFactory.Black(), new("a3"))]
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("O-Oxa3");
    }

    [Fact]
    public void CalculateSan_includes_promotion_notation()
    {
        Move move = new(
            new("e7"),
            new("e8"),
            PieceFactory.White(PieceType.Pawn),
            promotesTo: PieceType.Queen
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("e8=Q");
    }

    [Fact]
    public void CalculateSan_appends_hash_for_regular_move_with_kind_capture()
    {
        Move move = new(new("e4"), new("e5"), PieceFactory.White(PieceType.Rook));

        var san = _calculator.CalculateSan(move, [move], isKingCapture: true);

        san.Should().Be("Re5#");
    }

    [Theory]
    [InlineData(SpecialMoveType.KingsideCastle, "O-O#")]
    [InlineData(SpecialMoveType.QueensideCastle, "O-O-O#")]
    public void CalculateSan_appends_hash_for_castle_with_king_capture(
        SpecialMoveType moveType,
        string expectedSan
    )
    {
        Move move = new(
            new("e1"),
            new("g1"),
            PieceFactory.White(PieceType.King),
            specialMoveType: moveType
        );

        var san = _calculator.CalculateSan(move, [move], isKingCapture: true);

        san.Should().Be(expectedSan);
    }

    [Fact]
    public void CalculateSan_handles_il_vaticano()
    {
        Move move = new(
            new("c4"),
            new("f4"),
            PieceFactory.White(PieceType.Bishop),
            specialMoveType: SpecialMoveType.IlVaticano
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("B-O-O-B");
    }

    [Fact]
    public void CalculateSan_appends_intermediate_squares()
    {
        Move move = new(
            new("a1"),
            new("g6"),
            PieceFactory.White(PieceType.Checker),
            intermediateSquares: [new("b2"), new("d4"), new("f6")]
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("C~b2~d4~f6g6");
    }
}
