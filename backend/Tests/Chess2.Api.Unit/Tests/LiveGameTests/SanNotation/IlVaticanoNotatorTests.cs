using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.SanNotation.Notators;
using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests.SanNotation;

public class IlVaticanoNotatorTests
{
    private readonly IlVaticanoNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.IlVaticano);

    [Fact]
    public void Notate_uses_il_vaticano_notation()
    {
        Move move = new(
            new("c4"),
            new("f4"),
            PieceFactory.White(PieceType.Bishop),
            specialMoveType: SpecialMoveType.IlVaticano
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("B-O-O-B");
    }
}
