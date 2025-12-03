using AnarchyChess.Api.GameSnapshot.Models;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameSnapshotTests;

public class TimeControlSettingsTests
{
    [Fact]
    public void Constructor_converts_request()
    {
        TimeControlSettingsRequest request = new(BaseSeconds: 12345, IncrementSeconds: 6789);

        TimeControlSettings result = new(request);

        result
            .Should()
            .Be(
                new TimeControlSettings(
                    BaseSeconds: request.BaseSeconds,
                    IncrementSeconds: request.IncrementSeconds
                )
            );
    }

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
    public void Type_translates_the_game_length_to_a_time_control(
        int seconds,
        TimeControl expectedTimeControl
    )
    {
        TimeControlSettings timeControl = new(BaseSeconds: seconds, IncrementSeconds: 0);
        timeControl.Type.Should().Be(expectedTimeControl);
    }
}
