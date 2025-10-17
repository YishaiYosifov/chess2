using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.SanNotation.Notators;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests.SanNotation;

public class KingsideCastleNotatorTests
{
    private readonly KingsideCastleNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.KingsideCastle);

    [Fact]
    public void Notate_uses_kingside_castle_notation()
    {
        Move move = new(
            new("f1"),
            new("h1"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.KingsideCastle
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("O-O");
    }

    [Fact]
    public void Notate_adds_captures()
    {
        Move move = new(
            new("f1"),
            new("h1"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.KingsideCastle,
            captures: [new MoveCapture(PieceFactory.Black(), new("g1"))]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("O-Oxg1");
    }
}
