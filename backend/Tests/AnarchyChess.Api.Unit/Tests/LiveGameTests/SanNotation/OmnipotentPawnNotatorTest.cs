using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.TestInfrastructure.Factories;
using FluentAssertions;
using System.Text;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests.SanNotation;

public class OmnipotentPawnNotatorTest
{
    private readonly OmnipotentPawnNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.OmnipotentPawnSpawn);

    [Fact]
    public void Notate_uses_destination_only()
    {
        var piece = PieceFactory.White();
        AlgebraicPoint position = new("h3");
        Move move = new(
            position,
            position,
            piece,
            captures: [new MoveCapture(piece, position)],
            pieceSpawns: [new PieceSpawn(PieceType.Pawn, GameColor.Black, position)],
            specialMoveType: SpecialMoveType.OmnipotentPawnSpawn
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("xh3");
    }
}
