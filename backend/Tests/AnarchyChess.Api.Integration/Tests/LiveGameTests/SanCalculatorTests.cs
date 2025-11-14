using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.SanNotation;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class SanCalculatorTests : BaseIntegrationTest
{
    private readonly ISanCalculator _calculator;

    public SanCalculatorTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _calculator = Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
    }

    [Fact]
    public void Constructor_throws_if_no_default_notator_is_provided()
    {
        var act = () =>
            new SanCalculator(
                new PieceToLetter(),
                [new KingsideCastleNotator(new PieceToLetter())]
            );

        act.Should().Throw<KeyNotFoundException>();
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

    [Fact]
    public void CalculateSan_uses_special_move_notator_if_necessary()
    {
        Move move = new(
            new("f1"),
            new("g1"),
            PieceFactory.White(PieceType.King),
            specialMoveType: SpecialMoveType.KingsideCastle
        );

        var san = _calculator.CalculateSan(move, [move]);

        san.Should().Be("O-O");
    }
}
