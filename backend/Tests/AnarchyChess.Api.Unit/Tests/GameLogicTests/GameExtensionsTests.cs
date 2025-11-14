using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests;

public class GameExtensionsTests : BaseUnitTest
{
    [Theory]
    [InlineData(GameColor.White, GameColor.Black)]
    [InlineData(GameColor.Black, GameColor.White)]
    public void Invert_correctly_inverts_color(GameColor color, GameColor expected)
    {
        var result = color.Invert();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(GameColor.White, "white", "black", "white")]
    [InlineData(GameColor.Black, "white", "black", "black")]
    public void Match_correctly_matches_color(
        GameColor color,
        string whenWhite,
        string whenBlack,
        string expected
    )
    {
        var result = color.Match(whenWhite, whenBlack);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(GameColor.White, "white", "black", "neutral", "white")]
    [InlineData(GameColor.Black, "white", "black", "neutral", "black")]
    [InlineData(null, "white", "black", "neutral", "neutral")]
    public void Match_nullable_correctly_matches_color(
        GameColor? color,
        string whenWhite,
        string whenBlack,
        string whenNeutral,
        string expected
    )
    {
        var result = color.Match(whenWhite, whenBlack, whenNeutral);
        result.Should().Be(expected);
    }
}
