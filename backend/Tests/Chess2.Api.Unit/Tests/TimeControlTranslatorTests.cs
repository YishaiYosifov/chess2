using Chess2.Api.Models;
using Chess2.Api.Services;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests;

public class TimeControlTranslatorTests : BaseUnitTest
{
    private readonly TimeControlTranslator _timeControlTranslator = new();

    [Theory]
    [InlineData(60, TimeControl.Bullet)]
    [InlineData(180, TimeControl.Bullet)]
    [InlineData(181, TimeControl.Blitz)]
    [InlineData(240, TimeControl.Blitz)]
    [InlineData(300, TimeControl.Blitz)]
    [InlineData(301, TimeControl.Rapid)]
    [InlineData(600, TimeControl.Rapid)]
    [InlineData(1200, TimeControl.Rapid)]
    [InlineData(123123, TimeControl.Classical)]
    public void FromSeconds_translates_the_game_length_to_a_time_control(
        int seconds,
        TimeControl expectedTimeControl
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(seconds);
        timeControl.Should().Be(expectedTimeControl);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-123)]
    public void FromSeconds_throws_an_exception_when_the_game_length_is_invalid(int seconds)
    {
        Action act = () => _timeControlTranslator.FromSeconds(seconds);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
