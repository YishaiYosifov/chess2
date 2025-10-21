using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.SanNotation.Notators;
using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests.SanNotation;

public class VerticalCastleNotatorTest
{
    private readonly VerticalCastleNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.VerticalCastle);

    [Fact]
    public void Notate_uses_vertical_castle_notation()
    {
        Move move = new(
            new("f1"),
            new("f3"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.VerticalCastle
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("O-O-O-O-O-O");
    }

    [Fact]
    public void Notate_adds_captures()
    {
        Move move = new(
            new("f1"),
            new("f3"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.VerticalCastle,
            captures: [new MoveCapture(PieceFactory.Black(), new("f2"))]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("O-O-O-O-O-Oxf2");
    }
}
