using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameResultDescriberTests
{
    private readonly GameResultDescriber _describer = new();

    [Theory]
    [InlineData(GameColor.White, GameResult.WhiteWin, "White Won by King Capture")]
    [InlineData(GameColor.Black, GameResult.BlackWin, "Black Won by King Capture")]
    public void KingCaptured_returns_the_correct_status(
        GameColor winner,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.KingCaptured(winner);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.Aborted, "Game Aborted by White")]
    [InlineData(GameColor.Black, GameResult.Aborted, "Game Aborted by Black")]
    public void Aborted_returns_the_correct_status(
        GameColor abortedBy,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Aborted(abortedBy);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin, "Black Won by Resignation")]
    [InlineData(GameColor.Black, GameResult.WhiteWin, "White Won by Resignation")]
    public void Resignation_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Resignation(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin, "Black Won by Timeout")]
    [InlineData(GameColor.Black, GameResult.WhiteWin, "White Won by Timeout")]
    public void Timeout_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Timeout(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Fact]
    public void DrawByAgreement_returns_the_correct_status()
    {
        var result = _describer.DrawByAgreement();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by Agreement"));
    }

    [Fact]
    public void ThreeFold_returns_the_correct_status()
    {
        var result = _describer.ThreeFold();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by 3 Fold Repetition"));
    }

    [Fact]
    public void FiftyMoves_returns_the_correct_status()
    {
        var result = _describer.FiftyMoves();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by 50 Moves Rule"));
    }
}
