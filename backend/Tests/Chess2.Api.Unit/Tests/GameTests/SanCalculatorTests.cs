using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class SanCalculatorTests
{
    private readonly SanCalculator _calculator = new(new PieceToLetter());

    [Fact]
    public void CalculateSan_doesnt_disambiguate_when_not_necessary()
    {
        var move = new Move(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        var legalMoves = new List<Move> { move };

        var san = _calculator.CalculateSan(move, legalMoves);

        san.Should().Be("Hc3");
    }

    [Fact]
    public void CalculateSan_disambiguates_by_file()
    {
        var move1 = new Move(new("b1"), new("d2"), PieceFactory.White(PieceType.Horsey));
        var move2 = new Move(new("f1"), new("d2"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move1, [move1, move2]);

        san.Should().Be("Hbd2");
    }

    [Fact]
    public void CalculateSan_disambiguates_by_rank()
    {
        var move1 = new Move(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        var move2 = new Move(new("b6"), new("c3"), PieceFactory.White(PieceType.Horsey));
        var san = _calculator.CalculateSan(move1, [move1, move2]);

        san.Should().Be("H1c3");
    }

    [Fact]
    public void CalculateSan_disambiguates_by_both_file_and_rank()
    {
        var move1 = new Move(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        var move2 = new Move(new("b4"), new("c3"), PieceFactory.White(PieceType.Horsey));
        var move3 = new Move(new("e1"), new("c3"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move1, [move1, move2, move3]);

        san.Should().Be("Hb1c3");
    }

    [Fact]
    public void CalculateSan_doesnt_disambiguate_with_the_same_piece_type_on_different_destination()
    {
        var move = new Move(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        var otherMove = new Move(new("d2"), new("d4"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move, [move, otherMove]);

        san.Should().Be("Hc3");
    }

    [Fact]
    public void CalculateSan_doesnt_disambiguate_with_different_piece_type_to_same_destination()
    {
        var move = new Move(new("e2"), new("e4"), PieceFactory.White(PieceType.Rook));
        var otherMove = new Move(new("e7"), new("e4"), PieceFactory.White(PieceType.Horsey));

        var san = _calculator.CalculateSan(move, [move, otherMove]);

        san.Should().Be("Re4");
    }
}
