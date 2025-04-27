using AutoFixture;
using Chess2.Api.Models;
using Chess2.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit.Tests;

public class TimeControlTranslatorTests : BaseUnitTest
{
    private readonly IOptions<AppSettings> _settings;
    private readonly TimeControlTranslator _timeControlTranslator;

    public TimeControlTranslatorTests()
    {
        _settings = Fixture.Create<IOptions<AppSettings>>();
        _timeControlTranslator = new(_settings);
    }

    [Theory]
    [InlineData(60, TimeControl.Bullet)]
    [InlineData(300, TimeControl.Blitz)]
    [InlineData(600, TimeControl.Rapid)]
    [InlineData(61, TimeControl.Blitz)]
    [InlineData(301, TimeControl.Rapid)]
    [InlineData(123123, TimeControl.Rapid)]
    public void Correct_time_control_is_translated_from_seconds(int seconds, TimeControl expectedTimeControl)
    {
        var timeControl = _timeControlTranslator.FromSeconds(seconds);
        timeControl.Should().Be(expectedTimeControl);
    }
}
