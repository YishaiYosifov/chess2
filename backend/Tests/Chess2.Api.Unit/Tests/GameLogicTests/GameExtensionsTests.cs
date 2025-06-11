using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests;

public class GameExtensionsTests : BaseUnitTest
{
    [Theory]
    [InlineData(GameColor.White, GameColor.Black)]
    [InlineData(GameColor.Black, GameColor.White)]
    public void Invert_should_invert_game_color(GameColor color, GameColor expected)
    {
        var result = color.Invert();
        result.Should().Be(expected);
    }
}
