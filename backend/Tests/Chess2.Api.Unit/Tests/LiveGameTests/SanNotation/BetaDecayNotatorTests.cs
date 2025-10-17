using System.Text;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.SanNotation.Notators;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests.SanNotation;

public class BetaDecayNotatorTests
{
    private readonly BetaDecayNotator _notator = new(new PieceToLetter());

    [Fact]
    public void HandlesMoveType_is_correct() =>
        _notator.HandlesMoveType.Should().Be(SpecialMoveType.RadioactiveBetaDecay);

    [Fact]
    public void Notate_uses_beta_decay_notation()
    {
        Move move = new(
            new("a1"),
            new("a1"),
            PieceFactory.White(PieceType.Queen),
            specialMoveType: SpecialMoveType.RadioactiveBetaDecay,
            pieceSpawns:
            [
                new PieceSpawn(PieceType.SterilePawn, GameColor.White, new("j1")),
                new PieceSpawn(PieceType.Bishop, GameColor.White, new("i1")),
                new PieceSpawn(PieceType.Rook, GameColor.White, new("h1")),
            ]
        );

        StringBuilder sb = new();
        _notator.Notate(move, [move], sb);

        sb.ToString().Should().Be("Qβj1Bi1Rh1");
    }
}
