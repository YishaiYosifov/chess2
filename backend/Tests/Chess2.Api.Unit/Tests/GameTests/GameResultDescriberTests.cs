using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameResultDescriberTests
{
    private readonly GameResultDescriber _describer = new();

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
