using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.SanNotation.Notators;
using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests.SanNotation;

public class QueensideCastleNotatorTests
{
    private readonly QueensideCastleNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.QueensideCastle);

    [Fact]
    public void Notate_uses_queenside_castle_notation()
    {
        Move move = new(
            new("f1"),
            new("d1"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.QueensideCastle
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("O-O-O");
    }

    [Fact]
    public void Notate_adds_captures()
    {
        Move move = new(
            new("f1"),
            new("d1"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.QueensideCastle,
            captures: [new MoveCapture(PieceFactory.Black(), new("e1"))]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("O-O-Oxe1");
    }
}
