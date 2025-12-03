using System.Text;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests.SanNotation;

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
