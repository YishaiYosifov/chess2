using System.Text;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests.SanNotation;

public class RegularNotatorTests
{
    private readonly RegularNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.None);

    [Fact]
    public void Notate_doesnt_disambiguate_when_not_necessary()
    {
        Move move = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("Hc3");
    }

    [Fact]
    public void Notate_disambiguates_by_file()
    {
        Move move1 = new(new("b1"), new("d2"), PieceFactory.White(PieceType.Horsey));
        Move move2 = new(new("f1"), new("d2"), PieceFactory.White(PieceType.Horsey));

        StringBuilder sb = new();
        _notator.Notate(move1, [move1, move2], sb);

        sb.ToString().Should().Be("Hbd2");
    }

    [Fact]
    public void Notate_disambiguates_by_rank()
    {
        Move move1 = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move move2 = new(new("b6"), new("c3"), PieceFactory.White(PieceType.Horsey));

        StringBuilder sb = new();
        _notator.Notate(move1, [move1, move2], sb);

        sb.ToString().Should().Be("H1c3");
    }

    [Fact]
    public void Notate_disambiguates_by_both_file_and_rank()
    {
        Move move1 = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move move2 = new(new("b4"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move move3 = new(new("e1"), new("c3"), PieceFactory.White(PieceType.Horsey));

        StringBuilder sb = new();
        _notator.Notate(move1, [move1, move2, move3], sb);

        sb.ToString().Should().Be("Hb1c3");
    }

    [Fact]
    public void Notate_doesnt_disambiguate_with_the_same_piece_type_on_different_destination()
    {
        Move move = new(new("b1"), new("c3"), PieceFactory.White(PieceType.Horsey));
        Move otherMove = new(new("d2"), new("d4"), PieceFactory.White(PieceType.Horsey));

        StringBuilder sb = new();
        _notator.Notate(move, [move, otherMove], sb);

        sb.ToString().Should().Be("Hc3");
    }

    [Fact]
    public void Notate_doesnt_disambiguate_with_different_piece_type_to_same_destination()
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White(PieceType.Rook));
        Move otherMove = new(new("e7"), new("e4"), PieceFactory.White(PieceType.Horsey));

        StringBuilder sb = new();
        _notator.Notate(move, [move, otherMove], sb);

        sb.ToString().Should().Be("Re4");
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    public void Notate_doesnt_add_the_piece_letter_for_pawns(PieceType pawnType)
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White(pawnType));

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("e4");
    }

    [Fact]
    public void Notate_includes_destination_when_it_is_not_a_capture()
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

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("Re5xc4xd5");
    }

    [Fact]
    public void Notate_adds_the_destination_to_the_start_when_multi_capturing()
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

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("Rxd5xc4");
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.UnderagePawn)]
    public void Notate_handles_pawn_multi_capture_and_adds_file_letter(PieceType pawnType)
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

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("exf6xf5");
    }

    [Fact]
    public void Notate_handles_single_capture_when_destination_is_capture()
    {
        Move move = new(
            new("e5"),
            new("f6"),
            PieceFactory.White(PieceType.Pawn),
            captures: [new MoveCapture(PieceFactory.Black(), new("f6"))]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("exf6");
    }

    [Fact]
    public void CalculateSan_appends_intermediate_squares()
    {
        Move move = new(
            new("a1"),
            new("g6"),
            PieceFactory.White(PieceType.Checker),
            intermediateSquares:
            [
                new(new("b2"), IsCapture: true),
                new(new("d4"), IsCapture: false),
                new(new("f6"), IsCapture: true),
            ]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("C~b2~d4~f6g6");
    }
}
