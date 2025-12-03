using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameSnapshotTests;

public class TimeControlTranslatorTests : BaseUnitTest
{
    private readonly TimeControlTranslator _timeControlTranslator = new();

    [Theory]
    [InlineData(60, TimeControl.Bullet)]
    [InlineData(179, TimeControl.Bullet)]
    [InlineData(180, TimeControl.Blitz)]
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
