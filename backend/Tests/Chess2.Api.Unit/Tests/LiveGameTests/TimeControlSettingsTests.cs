using Chess2.Api.GameSnapshot.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class TimeControlSettingsTests
{
    [Theory]
    [InlineData(0, 0, "0+0")]
    [InlineData(60, 1, "60+1")]
    [InlineData(1800, 30, "1800+30")]
    [InlineData(10, 0, "10+0")]
    [InlineData(600, 10, "600+10")]
    public void ToShortString_formats_the_time_control_correctly(
        int baseSeconds,
        int incrementSeconds,
        string expected
    )
    {
        var timeControl = new TimeControlSettings(baseSeconds, incrementSeconds);

        var result = timeControl.ToShortString();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("0+0", 0, 0)]
    [InlineData("60+1", 60, 1)]
    [InlineData("1800+30", 1800, 30)]
    [InlineData("10+0", 10, 0)]
    [InlineData("600+10", 600, 10)]
    public void FromShortString_parses_the_time_control_correctly(
        string input,
        int expectedBase,
        int expectedIncrement
    )
    {
        var result = TimeControlSettings.FromShortString(input);

        result.BaseSeconds.Should().Be(expectedBase);
        result.IncrementSeconds.Should().Be(expectedIncrement);
    }

    [Theory]
    [InlineData("300")] // Missing increment
    [InlineData("300+")] // Missing increment value
    [InlineData("+5")] // Missing base
    [InlineData("")] // Empty string
    [InlineData("300+5+10")] // Too many parts
    [InlineData("abc+5")] // Invalid base
    [InlineData("300+abc")] // Invalid increment
    [InlineData("300.5+5")] // Decimal base
    [InlineData("300+5.5")] // Decimal increment
    [InlineData("300-5")] // Wrong separator
    public void FromShortString_throws_ArgumentException_for_invalid_formats(string input)
    {
        var act = () => TimeControlSettings.FromShortString(input);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Invalid time control format. Expected format is 'base+increment'.");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(60, 1)]
    [InlineData(1800, 30)]
    [InlineData(3600, 0)]
    [InlineData(180, 2)]
    public void FromShortString_keeps_equality_after_a_round_trip(
        int baseSeconds,
        int incrementSeconds
    )
    {
        var original = new TimeControlSettings(baseSeconds, incrementSeconds);

        var shortString = original.ToShortString();
        var roundTrip = TimeControlSettings.FromShortString(shortString);

        roundTrip.Should().Be(original);
    }
}
