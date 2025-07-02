using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameResultDescriberTests
{
    private readonly GameResultDescriber _describer = new();

    [Theory]
    [InlineData(GameColor.White, "Game Aborted by White")]
    [InlineData(GameColor.Black, "Game Aborted by Black")]
    public void Aborted_returns_the_correct_message(GameColor by, string expected)
    {
        var result = _describer.Aborted(by);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(GameColor.White, "Black Won by Resignation")]
    [InlineData(GameColor.Black, "White Won by Resignation")]
    public void Resignation_returns_the_correct_message(GameColor loser, string expected)
    {
        var result = _describer.Resignation(loser);
        result.Should().Be(expected);
    }
}
